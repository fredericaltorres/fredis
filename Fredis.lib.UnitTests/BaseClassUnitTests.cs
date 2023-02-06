namespace Fredis.lib.UnitTests
{
    public class BaseClassUnitTests : IDisposable
    {
        public string url => Environment.GetEnvironmentVariable("fredis_url");
        public string password = Environment.GetEnvironmentVariable("fredis_password");
        public bool ssl = true;
        public int timeOut = 10;

        protected FredisManager _fredisManager = null;

        protected BaseClassUnitTests()
        {
            _fredisManager = new FredisManager(url, password, ssl, timeOut);
        }

        protected string DeleteKeyAndCheck(string key)
        {
            string result;
            _fredisManager.DeleteKey(key);
            result = _fredisManager.GetValue(key);
            Assert.Equal(null, result);
            return result;
        }

        public void Dispose()
        {
            _fredisManager.Close();
        }
    }
}