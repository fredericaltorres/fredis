# fredis
.NET helper library for REDIS

    public class IFredisManager
    {
        // Initialization
        FredisManager(string url, string password, bool ssl, int timeOut);
        void Close();

        // Redis object
        IServer Server { get; }
        IDatabase Database;

        // Set Key
        bool SetKey<T>(string key, T val, int ttlInMinutes = 60);
        
        bool SetList<T>(string key, List<T> val, int ttlInMinutes = 60);

        bool SetDictionary<T>(string key, Dictionary<string, T> val, int ttlInMinutes = 60);
            bool SetDictionaryItemKey<T>(string key, string id, T val);

        bool SetTTL(string key, int ttlInMinutes);
        bool KeyExists(string key);
        
        bool DeleteKey(string key);
        bool DeleteKeys(List<string> keys);
        bool DeleteKeys(List<fRedisItem> entries);

        // Get Key
        T Get<T>(string key, T defaultValue = default);
        List<T> GetList<T>(string key);
        Dictionary<string, T> GetDictionary<T>(string key);
            T GetDictionaryItem<T>(string key, string id);
                    
        // Querying For Keys
        List<string> QueryKeys(string pattern, bool sort = true);
        List<fRedisItem> GetKeys(string pattern, bool sort = true);

        FredisQueryResult GetReceivedMessage();
        FredisManager.RedisValueEx GetValueAsTextEx(string key, RedisType redisType);
    }