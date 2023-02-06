﻿using Newtonsoft.Json;
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

    public class FredisItem
    {
        public string Key { get; set; }
        public string TextValue { get; set; }
        public string Type { get; set; }
        public string TTL { get; set; }
        public long Length { get; set; }

        public RedisValue Value { get; set; }
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
        public bool Succeeded;
        public long DurationInMilliSeconds;
        public string Error;
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

        private List<string> GetAllSortedKeys(string pattern)
        {
            var srv = this.Server;
            var keys = srv.Keys(pattern: pattern).Select(s => s.ToString()).ToList();
            keys.Sort();
            return keys;
        }

        public List<FredisItem> GetKeys(string pattern)
        {
            var keys = this.GetAllSortedKeys(pattern);
            var redisItem = new List<FredisItem>();
            foreach (var key in keys)
            {
                var keyType = this.Database.KeyType(key);
                TimeSpan? ttl = this.Database.KeyTimeToLive(key);
                var ttlString = "--.--:--:--";
                if (ttl.HasValue)
                    ttlString = ttl.Value.ToString(@"dd\.hh\:mm\:ss");

                var valueEx = this.GetValueAsTextEx(key, keyType);

                redisItem.Add(new FredisItem { Key = key, TextValue = valueEx.Text, TTL = ttlString, Type = keyType.ToString(), Length = valueEx.Length });
            }
            return redisItem;
        }

        private bool IsInteger(string k)
        {
            int i;
            return int.TryParse(k, out i);
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
        public Dictionary<string, long> GetSortedSetValue(string key)
        {
            var d = new Dictionary<string, long>();
            RedisValue[] sortedValues = Database.SortedSetRangeByRank(key);
            long len = 0;
            foreach (var s in sortedValues)
            {
                var score = Database.SortedSetScore(key, s);
                if (score.HasValue)
                    d.Add(s, long.Parse(score.Value.ToString()));
                else
                    d.Add(s, 0);
            }

            return d;
        }

        public Dictionary<string, string> GetDictionaryValue(string key)
        {
            var d = new Dictionary<string, string>();
            var hashEntries = Database.HashGetAll(key).ToList();

            foreach (var e in hashEntries)
                d.Add(e.Name, e.Value);

            return d;
        }

        public List<string> GetListValue(string key)
        {
            var l = new List<string>();
            var count = Database.ListLength(key);
            for (var i = count - 1; i >= 0; i--)
            {
                var item = Database.ListGetByIndex(key, i);
                l.Add(item);
            }

            return l;
        }

        public DateTime GetValue(string key, DateTime defaultValue)
        {
            var s = GetValue(key, null as string);
            if (s == null)
                return defaultValue;
            return Deserialize<DateTime>(s);
        }
        public long GetValue(string key, long defaultValue)
        {
            var s = GetValue(key, null as string);
            if (s == null)
                return defaultValue;
            return long.Parse(s);
        }

        public string GetValue(string key, string defaultValue = null)
        {
            RedisValue rdv = Database.StringGet(key);
            if (rdv.IsNull)
                return defaultValue;

            return rdv.ToString();
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

        public bool CreateListKey(string key, List<string> val, int ttlInMinutes = 60)
        {
            this.DeleteKeyIfExits(key);

            foreach (var item in val)
                this.Database.ListLeftPush(key, item);

            var r0 = this.Database.KeyExpire(key, DateTime.Now.AddMinutes(ttlInMinutes));

            return r0;
        }

        public List<string> GetListKey(string key)
        {
            var l = new List<string>();
            var count = Database.ListLength(key);
            for (var i = count - 1; i >= 0; i--)
            {
                var item = Database.ListGetByIndex(key, i);
                l.Add(item);
            }
            return l;
        }

        public bool CreateKey(string key, string val, int ttlInMinutes = 60)
        {
            var r0 = false;
            this.DeleteKeyIfExits(key);

            var ts = new TimeSpan(0, ttlInMinutes, 0);
            if (ttlInMinutes == 0)
                r0 = this.Database.StringSet(key, val);
            else
                r0 = this.Database.StringSet(key, val, ts);
            return r0;
        }


        private string Serialize(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        private T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public bool CreateKey(string key, DateTime val, int ttlInMinutes = 60) 
        { 
            return CreateKey(key, Serialize(val), ttlInMinutes);
        }

        // https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/RedisValue.csC
        // https://stackoverflow.com/questions/32274113/difference-between-storing-integers-and-strings-in-redis
        public bool CreateKey(string key, System.Int16 val, int ttlInMinutes = 60) { return CreateKey(key, val.ToString(), ttlInMinutes); }
        public bool CreateKey(string key, System.UInt16 val, int ttlInMinutes = 60) { return CreateKey(key, val.ToString(), ttlInMinutes); }
        public bool CreateKey(string key, System.Int64 val, int ttlInMinutes = 60) { return CreateKey(key, val.ToString(), ttlInMinutes); }
        public bool CreateKey(string key, System.UInt64 val, int ttlInMinutes = 60) { return CreateKey(key, val.ToString(), ttlInMinutes); }
        public bool CreateKey(string key, System.Int32 val, int ttlInMinutes = 60) { return CreateKey(key, val.ToString(), ttlInMinutes); }
        public bool CreateKey(string key, System.UInt32 val, int ttlInMinutes = 60) { return CreateKey(key, val.ToString(), ttlInMinutes); }

        public FredisQueryResult GetReceivedMessage()
        {
            var rr = new FredisQueryResult();

            if (_receivedMessages.Count > 0)
            {
                rr.Text = "";// _receivedMessages;
            }

            return rr;
        }

        private void DeleteKeyIfExits(string key)
        {
            if (this.Database.KeyExists(key))
            {
                this.DeleteKey(key);
            }
        }
    }
}

