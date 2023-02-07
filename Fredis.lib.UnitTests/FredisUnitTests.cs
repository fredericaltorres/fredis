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
            var result = base._fredisManager.GetValue<string>(key);
            Assert.Equal(val, result);
            DeleteKeyAndCheck(key);
        }

        [Fact]
        public void Create_Get_Delete_Double_Key()
        {
            var val = 123.456;
            base._fredisManager.CreateKey(key, val, 1);
            var result = base._fredisManager.GetValue<double>(key, 0.0);
            Assert.Equal(val, result);
            DeleteKeyAndCheck(key);
        }

        [Fact]
        public void Create_Get_Delete_Integer_Key()
        {
            var val = 123;
            base._fredisManager.CreateKey(key, val, 1);
            var result = base._fredisManager.GetValue<int>(key, 0);
            Assert.Equal(val, result);
            DeleteKeyAndCheck(key);
        }

        [Fact]
        public void Create_Get_Delete_DateTime_Key()
        {
            var val = DateTime.Now;
            base._fredisManager.CreateKey(key, val, 1);
            var result = base._fredisManager.GetValue<DateTime>(key, DateTime.Now);
            Assert.Equal(val, result);
            DeleteKeyAndCheck(key);
            
        }

        [Fact]
        public void Create_GetWithDefaultValue_Delete_DateTime_Key()
        {
            var val = new DateTime(1964,12,1);
            DeleteKeyAndCheck(key);
            var result = base._fredisManager.GetValue<DateTime>(key, val);
            Assert.Equal(val, result);
        }

        [Fact]
        public void Create_Get_List_Key_String()
        {
            var list = new List<string>() { "A", "B", "C" };
            base._fredisManager.CreateListKey<string>(key, list);
            var result = base._fredisManager.GetListValue(key);
            Assert.Equal(list, result);
        }

        [Fact]
        public void Create_Get_List_Key_Int()
        {
            var list = new List<int>() { 1,2,3 };
            base._fredisManager.CreateListKey<int>(key, list);
            var result = base._fredisManager.GetListValue<int>(key);
            Assert.Equal(list, result);
        }

        [Fact]
        public void Create_Get_List_Key_Double()
        {
            var list = new List<double>() { 1.1, 2.2, 3.3 };
            base._fredisManager.CreateListKey<double>(key, list);
            var result = base._fredisManager.GetListValue<double>(key);
            Assert.Equal(list, result);
        }

        [Fact]
        public void Create_Get_List_Key_Decimal()
        {
            var list = new List<decimal>() { 1.1M, 2.2M, 3.3M };
            base._fredisManager.CreateListKey<decimal>(key, list);
            var result = base._fredisManager.GetListValue<decimal>(key);
            Assert.Equal(list, result);
        }
    }
}