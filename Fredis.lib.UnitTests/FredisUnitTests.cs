namespace Fredis.lib.UnitTests
{
    public class FredisUnitTests : BaseClassUnitTests
    {
        const string _mainTestKey = "ut_Key01";

        public FredisUnitTests() : base()
        {
        }


        [Fact]
        public void KeyExists()
        {
            DeleteKeyAndCheck(_mainTestKey);

            var val = "Hello";
            var result = base._fredisManager.KeyExists(_mainTestKey);
            Assert.False(result);

            base._fredisManager.CreateKey(_mainTestKey, val, 1);
            result = base._fredisManager.KeyExists(_mainTestKey);
            Assert.True(result);
            DeleteKeyAndCheck(_mainTestKey);
        }

        [Fact]
        public void Create_Get_Delete_String_Key()
        {
            var val = "Hello";
            base._fredisManager.CreateKey(_mainTestKey, val, 1);
            var result = base._fredisManager.GetValue<string>(_mainTestKey);
            Assert.Equal(val, result);
            DeleteKeyAndCheck(_mainTestKey);
        }

        [Fact]
        public void Create_Get_Delete_Double_Key()
        {
            var val = 123.456;
            base._fredisManager.CreateKey(_mainTestKey, val, 1);
            var result = base._fredisManager.GetValue<double>(_mainTestKey, 0.0);
            Assert.Equal(val, result);
            DeleteKeyAndCheck(_mainTestKey);
        }

        [Fact]
        public void Create_Get_Delete_Integer_Key()
        {
            var val = 123;
            base._fredisManager.CreateKey(_mainTestKey, val, 1);
            var result = base._fredisManager.GetValue<int>(_mainTestKey, 0);
            Assert.Equal(val, result);
            DeleteKeyAndCheck(_mainTestKey);
        }

        [Fact]
        public void Create_Get_Delete_DateTime_Key()
        {
            var val = DateTime.Now;
            base._fredisManager.CreateKey(_mainTestKey, val, 1);
            var result = base._fredisManager.GetValue<DateTime>(_mainTestKey, DateTime.Now);
            Assert.Equal(val, result);
            DeleteKeyAndCheck(_mainTestKey);
            
        }

        [Fact]
        public void Create_GetWithDefaultValue_Delete_DateTime_Key()
        {
            var val = new DateTime(1964,12,1);
            DeleteKeyAndCheck(_mainTestKey);
            var result = base._fredisManager.GetValue<DateTime>(_mainTestKey, val);
            Assert.Equal(val, result);
        }

        [Fact]
        public void Create_Get_List_Key_String()
        {
            var list = new List<string>() { "A", "B", "C" };
            base._fredisManager.CreateListKey<string>(_mainTestKey, list);
            var result = base._fredisManager.GetListValue(_mainTestKey);
            Assert.Equal(list, result);
        }

        [Fact]
        public void Create_Get_List_Key_Int()
        {
            var list = new List<int>() { 1,2,3 };
            base._fredisManager.CreateListKey<int>(_mainTestKey, list);
            var result = base._fredisManager.GetListValue<int>(_mainTestKey);
            Assert.Equal(list, result);
        }

        [Fact]
        public void Create_Get_List_Key_Double()
        {
            var list = new List<double>() { 1.1, 2.2, 3.3 };
            base._fredisManager.CreateListKey<double>(_mainTestKey, list);
            var result = base._fredisManager.GetListValue<double>(_mainTestKey);
            Assert.Equal(list, result);
        }

        [Fact]
        public void Create_Get_List_Key_Decimal()
        {
            var list = new List<decimal>() { 1.1M, 2.2M, 3.3M };
            base._fredisManager.CreateListKey<decimal>(_mainTestKey, list);
            var result = base._fredisManager.GetListValue<decimal>(_mainTestKey);
            Assert.Equal(list, result);
        }

        [Fact]
        public void Create_Get_Dictionary_Key_Int()
        {
            var dic = new Dictionary<string, int>() { ["A"]=1, ["B"]=2, ["C"]=3 };
            base._fredisManager.CreateDictionaryKey<int>(_mainTestKey, dic);
            var result = base._fredisManager.GetDictionaryValue<int>(_mainTestKey);
            Assert.Equal(dic, result);
        }
    }
}