namespace Fredis.lib.UnitTests
{
    public class BaseClassUnitTests : IDisposable
    {
        public string url => Environment.GetEnvironmentVariable("fredis_url");
        public string password = Environment.GetEnvironmentVariable("fredis_password");
        public bool ssl = true;
        public int timeOut = 30;

        protected FredisManager _fRedis = null;

        protected BaseClassUnitTests()
        {
            _fRedis = new FredisManager(url, password, ssl, timeOut);
        }

        protected string DeleteKeyAndCheck(string key)
        {
            string result;
            _fRedis.DeleteKey(key);
            result = _fRedis.Get<string>(key);
            Assert.Equal(null, result);
            return result;
        }

        public void Dispose()
        {
            _fRedis.Close();
        }
    }
}