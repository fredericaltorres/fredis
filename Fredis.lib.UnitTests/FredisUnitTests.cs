namespace Fredis.lib.UnitTests
{
    public class FredisUnitTests : BaseClassUnitTests
    {
        const string key = "ut_Key01";
        public FredisUnitTests() : base()
        {
        }

        [Fact]
        public void Create_Get_Delete_String_Key()
        {
            var val = "Hello";

            base._fredisManager.CreateKey(key, val, 1);
            var result = base._fredisManager.GetValue(key);
            Assert.Equal(val, result);
            DeleteKeyAndCheck(key);
        }

        [Fact]
        public void Create_Get_Delete_Integer_Key()
        {
            var val = 123;
            base._fredisManager.CreateKey(key, val, 1);
            var result = base._fredisManager.GetValue(key, 0);
            Assert.Equal(val, result);

            DeleteKeyAndCheck(key);
        }

        [Fact]
        public void Create_Get_Delete_DateTime_Key()
        {
            var val = DateTime.Now;
            base._fredisManager.CreateKey(key, val, 1);
            var result = base._fredisManager.GetValue(key, DateTime.Now);
            Assert.Equal(val, result);

            DeleteKeyAndCheck(key);
        }


        [Fact]
        public void Create_Get_List_Key()
        {
            var val = new List<string>() { "A", "B", "C" };

            base._fredisManager.CreateListKey(key, val);
            var result = base._fredisManager.GetListValue(key);
            Assert.Equal(val, result);
        }
    }
}