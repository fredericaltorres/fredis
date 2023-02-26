using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Fredis
{
    // https://stackexchange.github.io/StackExchange.Redis/KeysValues.html
    // http://tostring.it/2015/04/23/An-easy-way-to-use-StackExchange-Redis-copy/
    // https://redis.io/commands/type/

    public class fRedisItem
    {
        public string Key { get; set; }
        
        public RedisType Type { get; set; }
        public TimeSpan? TTL { get; set; }


        public string TTLToString()
        {
            var ttlString = "--.--:--:--";
            if (TTL.HasValue)
                ttlString = TTL.Value.ToString(@"dd\.hh\:mm\:ss");
            return ttlString;
        }
    }

    public class FredisReceivedMessage
    {
        public string Channel { get; set; }
        public string Message { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class FredisQueryResult
    {
        public string Text;
        public long DurationInMilliSeconds;
        public int RowCount;
    }

    public class FredisManager 
    {
        internal ConnectionMultiplexer _redisConnection;
        private ConfigurationOptions _connectionOptions;
        public List<FredisReceivedMessage> _receivedMessages = new List<FredisReceivedMessage>();
        public Dictionary<string, DateTime> _subscribedChannels = new Dictionary<string, DateTime>();

        public string ConnectionSummaryString;
        public IDatabase Database;

        public FredisManager(string url, string password, bool ssl, int timeOut)
        {
            _connectionOptions = new ConfigurationOptions
            {
                EndPoints = { url },
                Password = password,
                Ssl = ssl,
                AllowAdmin = true,
                AsyncTimeout = 1000 * timeOut,
                ConnectTimeout = 1000 * timeOut,
                SyncTimeout = 1000 * timeOut,
            };

            this.ConnectionSummaryString = $"url:{url}, ssl:{ssl}, timeout:{timeOut}";

            _redisConnection = ConnectionMultiplexer.Connect(_connectionOptions);
            Database = _redisConnection.GetDatabase();
        }

        public IServer Server
        {
            get
            {
                var endpoint = _redisConnection.GetEndPoints().First();
                return _redisConnection.GetServer(endpoint);
            }
        }

        public List<string> QueryKeys(string pattern, bool sort = true)
        {
            var srv = this.Server;
            var keys = srv.Keys(pattern: pattern).Select(s => s.ToString()).ToList();
            if(sort)
                keys.Sort();
            return keys;
        }

        public List<fRedisItem> GetKeys(string pattern, bool sort = true)
        {
            var keys = this.QueryKeys(pattern, sort);
            var redisItem = new List<fRedisItem>();
            foreach (var key in keys)
            {
                RedisType keyType = this.Database.KeyType(key);
                TimeSpan? ttl = this.Database.KeyTimeToLive(key);
                RedisValue rv = RedisValue.EmptyString;

                redisItem.Add(new fRedisItem
                {
                    Key = key,
                    TTL = ttl,
                    Type = keyType,
                });
            }
            return redisItem;
        }

        public void Close()
        {
            _redisConnection.Close();
            _redisConnection = null;
        }

        public class RedisValueEx
        {
            public string Text;
            public long Length;
            public bool Error;
        }

        public bool DeleteKeys(List<fRedisItem> entries)
        {
            foreach (var e in entries)
                if (!DeleteKey(e.Key))
                    return false;

            return true;
        }

        public bool DeleteKeys(List<string> keys)
        {
            foreach (var key in keys)
                if (!DeleteKey(key))
                    return false;

            return true;
        }

        public bool DeleteKey(string key)
        {
            try
            {
                if (Database.KeyDelete(key))
                    return true;
            }
            catch (Exception ex)
            {
            }
            return false;
        }
    

        public T GetDictionaryItem<T>(string key, string id)
        {
            var l = new Dictionary<string, T>();
            var v = Database.HashGet(key, id);
            T t = (T)Convert.ChangeType(v, typeof(T));
            return t;
        }

        public Dictionary<string, T> GetDictionary<T>(string key)
        {
            var l = new Dictionary<string, T>();
            var hashEntries = Database.HashGetAll(key).ToList();
            foreach (var e in hashEntries)
                l.Add(e.Name, (T)Convert.ChangeType(e.Value, typeof(T)));

            return l;
        }

        public List<T> GetList<T>(string key)
        {
            var l = new List<T>();
            var count = Database.ListLength(key);
            for (var i = count-1; i >= 0; i--)
            {
                var v = Database.ListGetByIndex(key, i);
                T t = (T)Convert.ChangeType(v, typeof(T));
                l.Add(t);
            }
            return l;
        }

        public void ListEnQueue<T>(string key, T t)
        {
            Database.ListLeftPush(key, t.ToString());
        }

        public T ListDeQueue<T>(string key)
        {
            RedisValue r = Database.ListRightPop(key);
            T t = (T)Convert.ChangeType(r, typeof(T));
            return t;
        }

        public void ListAdd<T>(string key, T t)
        {
            Database.ListLeftPush(key, t.ToString());
        }

        // https://stackoverflow.com/questions/31955977/how-to-store-list-element-in-redis-cache
        // https://github.com/thepirat000/CachingFramework.Redis

        public void ListInsert<T>(string key, T t, int position, bool insertAfter = true)
        {
            // Above implementation does not work
            var l = this.GetList<T>(key); 
            l.Insert(position, t);
            this.SetList(key, l);
            //if (insertAfter)
            //    Database.ListInsertAfter(key, position, t.ToString());
            //else
            //    Database.ListInsertBefore(key, position, t.ToString());
        }

        public T ListPop<T>(string key)
        {
            RedisValue r = Database.ListLeftPop(key);
            T t = (T)Convert.ChangeType(r, typeof(T));
            return t;
        }

        public T Remove<T>(string key, int position)
        {
            var l = this.GetList<T>(key);
            T r = l[position];
            l.RemoveAt(position);
            this.SetList(key, l);
            return r;
        }

        private DateTime? GetDateTimeValue(string key)
        {
            var s = Get(key, null as string);
            if (s == null)
                return null;
            return Deserialize<DateTime>(s);
        }

        public T Get<T>(string key, T defaultValue = default(T))
        {
            if (defaultValue is DateTime)
            {
                var v = GetDateTimeValue(key);
                if (v == null)
                {
                    return (T)Convert.ChangeType(defaultValue, typeof(T));
                }
                else
                {
                    return (T)Convert.ChangeType(v, typeof(T));
                }
            }

            var s = this.Database.StringGet(key);
            if (!s.HasValue)
                return defaultValue;

            return (T)Convert.ChangeType(s, typeof(T));
        }

        public RedisValueEx GetValueAsTextEx(string key, RedisType redisType)
        {
            try
            {
                switch (redisType)
                {
                    case RedisType.Hash:
                        {
                            var hashEntries = Database.HashGetAll(key).ToList();
                            var sb = new StringBuilder();
                            foreach (var e in hashEntries)
                                sb.Append($"{e.Name}: {e.Value}, ");
                            var t = string.Empty;
                            if (hashEntries.Count > 0)
                                t = sb.ToString().Substring(0, sb.Length - 2);
                            return new RedisValueEx { Text = t, Length = hashEntries.Count };
                        }
                    case RedisType.String:
                        {
                            RedisValue rdv = Database.StringGet(key);
                            return new RedisValueEx { Text = rdv.ToString(), Length = rdv.Length() };
                        }
                    case RedisType.List:
                        {
                            var count = Database.ListLength(key);
                            var sb = new StringBuilder();
                            for (var i = count - 1; i >= 0; i--)
                            {
                                var item = Database.ListGetByIndex(key, i);
                                sb.Append($"{item}, ");
                            }
                            var t = string.Empty;
                            if (count > 0)
                                t = sb.ToString().Substring(0, sb.Length - 2);

                            return new RedisValueEx { Text = t, Length = count };
                        }
                    case RedisType.SortedSet:
                        {
                            var sb = new StringBuilder();
                            RedisValue[] sortedValues = Database.SortedSetRangeByRank(key);
                            long len = 0;
                            foreach (var s in sortedValues)
                            {
                                len += s.Length();
                                var score = Database.SortedSetScore(key, s);
                                sb.Append($"{s}: {score}, ");
                            }
                            var t = string.Empty;
                            if (sortedValues.Length > 0)
                                t = sb.ToString().Substring(0, sb.Length - 2);
                            return new RedisValueEx { Text = t, Length = len };
                        }
                }
                return new RedisValueEx { Text = $"Unsupported key type:{redisType}, key:{key}", Error = true };
            }
            catch (Exception ex)
            {
                return new RedisValueEx { Text = $"{ex.GetType().Name}:{ex.Message}", Error = true };
            }
        }

        //public List<string> GetListKey(string key)
        //{
        //    var l = new List<string>();
        //    var count = Database.ListLength(key);
        //    for (var i = count - 1; i >= 0; i--)
        //    {
        //        var item = Database.ListGetByIndex(key, i);
        //        l.Add(item);
        //    }
        //    return l;
        //}

        public bool SetKey<T>(string key, T val, int ttlInMinutes = 60)
        {
            var r0 = false;
            this.DeleteKeyIfExits(key);

            var valAsString = __getStringRepresentation(val);

            var ts = new TimeSpan(0, ttlInMinutes, 0);
            if (ttlInMinutes == 0)
                r0 = this.Database.StringSet(key, valAsString);
            else
                r0 = this.Database.StringSet(key, valAsString, ts);
            return r0;
        }

        private string __getStringRepresentation<T>(T val)
        {
            var valAsString = string.Empty; // DateTime are serialized the JSON way
            if (val is DateTime)
                valAsString = Serialize(val);
            else
                valAsString = val.ToString();
            return valAsString;
        }

        public bool SetDictionary<T>(string key, Dictionary<string, T> val, int ttlInMinutes = 60)
        {
            this.DeleteKeyIfExits(key);
            var hashEntries = new List<HashEntry>();

            foreach (var e in val)
            {
                var valAsString = __getStringRepresentation(e.Value);
                hashEntries.Add(new HashEntry(e.Key, valAsString));
            }
            this.Database.HashSet(key, hashEntries.ToArray());
            return this.SetTTL(key, ttlInMinutes);
        }

        public bool SetDictionaryItemKey<T>(string key, string id, T val)
        {
            var valAsString = __getStringRepresentation(val);
            this.Database.HashSet(key, id, valAsString);
            return true;
        }

        public bool SetList<T>(string key, List<T> val, int ttlInMinutes = 60)
        {
            this.DeleteKeyIfExits(key);

            var redisValueList = new List<RedisValue>();

            foreach (var item in val)
                redisValueList.Add(__getStringRepresentation(item));

            this.Database.ListLeftPush(key, redisValueList.ToArray());

            return this.SetTTL(key, ttlInMinutes);
        }

        public bool SetTTL(string key, int ttlInMinutes)
        {
            return this.Database.KeyExpire(key, DateTime.Now.AddMinutes(ttlInMinutes));
        }

        //public bool CreateListKey(string key, List<string> val, int ttlInMinutes = 60)
        //{
        //    this.DeleteKeyIfExits(key);

        //    foreach (var item in val)
        //        this.Database.ListLeftPush(key, item);

        //    return SetTTL(key, ttlInMinutes);
        //}


        private string Serialize(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        private T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        // https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/RedisValue.csC
        // https://stackoverflow.com/questions/32274113/difference-between-storing-integers-and-strings-in-redis

        public FredisQueryResult GetReceivedMessage()
        {
            var rr = new FredisQueryResult();

            if (_receivedMessages.Count > 0)
            {
                rr.Text = "";// _receivedMessages;
            }

            return rr;
        }
        public bool KeyExists(string key)
        {
            return this.Database.KeyExists(key);
        }

        private void DeleteKeyIfExits(string key)
        {
            if (this.KeyExists(key))
            {
                this.DeleteKey(key);
            }
        }
    }
}


