namespace Fredis.lib.UnitTests
{
    public class FredisBaseClassUnitTests : IDisposable
    {
        public string url = "bskredisqa.redis.cache.windows.net:6380";
        public string password = "s1VeHEfu4XjZqHXryfa8C28ZizqVPCoCfNO0fHlTUzE=";
        public bool ssl = true;
        public int timeOut = 10;

        protected FredisManager _fredisManager = null;

        protected FredisBaseClassUnitTests()
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