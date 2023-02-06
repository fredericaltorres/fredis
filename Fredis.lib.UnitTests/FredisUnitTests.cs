namespace Fredis.lib.UnitTests
{
    public class FredisUnitTests : FredisBaseClassUnitTests
    {
        public FredisUnitTests() : base()
        {
        }

        [Fact]
        public void Create_Get_String_Key()
        {
            var key = "ut_Key01";
            var val = "Hello";

            base._fredisManager.CreateStringKey(key, val, 1);
            var result = base._fredisManager.GetStringValue(key);

            Assert.Equal(val, result);
        }

        [Fact]
        public void Create_Get_List_Key()
        {
            var key = "ut_KeyList01";
            var val = new List<string>() { "A", "B", "C" };

            base._fredisManager.CreateListKey(key, val);
            var result = base._fredisManager.GetListValue(key);
            Assert.Equal(val, result);
        }
    }
}