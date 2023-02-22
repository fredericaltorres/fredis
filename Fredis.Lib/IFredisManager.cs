using StackExchange.Redis;
using System.Collections.Generic;

namespace Fredis
{
    public interface IFredisManager
    {
        IServer Server { get; }

        void Close();
        bool CreateListKey(string key, List<string> val, int ttlInMinutes = 60);
        bool DeleteKey(string key);
        bool DeleteKeys(List<fRedisItem> entries);
        bool DeleteKeys(List<string> keys);
        T Get<T>(string key, T defaultValue = default);
        List<string> GetAllKeys(string pattern, bool sort = true);
        Dictionary<string, T> GetDictionary<T>(string key);
        T GetDictionaryItem<T>(string key, string id);
        List<fRedisItem> GetKeys(string pattern, bool addTextRepresentation = false, bool sort = true);
        List<T> GetList<T>(string key);
        List<string> GetListKey(string key);
        FredisQueryResult GetReceivedMessage();
        Dictionary<string, long> GetSortedSetValue(string key);
        FredisManager.RedisValueEx GetValueAsTextEx(string key, RedisType redisType);
        bool KeyExists(string key);
        bool SetDictionary<T>(string key, Dictionary<string, T> val, int ttlInMinutes = 60);
        bool SetDictionaryItemKey<T>(string key, string id, T val);
        bool SetKey<T>(string key, T val, int ttlInMinutes = 60);
        bool SetList<T>(string key, List<T> val, int ttlInMinutes = 60);
        bool SetTTL(string key, int ttlInMinutes);
    }
}