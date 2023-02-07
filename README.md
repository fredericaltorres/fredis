# fredis
.NET helper library for REDIS



    public interface IFredisManager
    {
        IServer Server { get; }
        void Close();

        bool CreateDictionaryKey<T>(string key, Dictionary<string, T> val, int ttlInMinutes = 60);
        bool CreateKey<T>(string key, T val, int ttlInMinutes = 60);
        bool CreateListKey<T>(string key, List<T> val, int ttlInMinutes = 60);

        bool DeleteKey(string key);
        bool KeyExists(string key);
        bool SetTTL(string key, int ttlInMinutes);

        Dictionary<string, T> GetDictionaryValue<T>(string key);
        List<FredisItem> GetKeys(string pattern);

        List<string> GetListKey(string key);
        List<string> GetListValue(string key);
        List<T> GetListValue<T>(string key);

        
        string GetValue(string key, string defaultValue = null);
        double GetValue(string key, double defaultValue);
        T GetValue<T>(string key, T defaultValue = default);

        
    }