using DynamicSugar;

namespace Fredis.lib.UnitTests
{
    public class FredisUnitTests : BaseClassUnitTests
    {
        const string _mainTestKey = "ut_Key001";
        const string multiTestKeyPrefix = "mut_Key_";

        public FredisUnitTests() : base()
        {
        }

        [Fact]
        public void Search2()
        {
            var result = base._fRedis.GetKeys($"fms*");
            Assert.True(result.Count > 0);
            var sb = new System.Text.StringBuilder(512);

            foreach (var i in result)
            {
                switch (i.Type)
                {
                    case StackExchange.Redis.RedisType.String:
                        sb.Append(base._fRedis.Get<string>(i.Key)).Append(", ");
                        break;
                }
            }
        }
        [Fact]
        public void Search()
        {
            var result = base._fRedis.Get<string>("fms-presentation-1248185269204943305");
        }

        [Fact]
        public void KeyExists()
        {
            DeleteKeyAndCheck(_mainTestKey);
            var val = "Hello";
            var result = base._fRedis.KeyExists(_mainTestKey);
            Assert.False(result);

            base._fRedis.SetKey(_mainTestKey, val, 1);
            result = base._fRedis.KeyExists(_mainTestKey);
            Assert.True(result);
            DeleteKeyAndCheck(_mainTestKey);
        }

        [Fact]
        public void Create_Get_Delete_String_Key()
        {
            var val = "Hello";
            base._fRedis.SetKey(_mainTestKey, val, 1);
            var result = base._fRedis.Get<string>(_mainTestKey);
            Assert.Equal(val, result);
            DeleteKeyAndCheck(_mainTestKey);
        }

        [Fact]
        public void Create_Get_Delete_Double_Key()
        {
            var val = 123.456;
            base._fRedis.SetKey(_mainTestKey, val, 1);
            var result = base._fRedis.Get<double>(_mainTestKey, 0.0);
            Assert.Equal(val, result);
            DeleteKeyAndCheck(_mainTestKey);
        }

        [Fact]
        public void Create_Get_Delete_Integer_Key()
        {
            var val = 123;
            base._fRedis.SetKey(_mainTestKey, val, 1);
            var result = base._fRedis.Get<int>(_mainTestKey, 0);
            Assert.Equal(val, result);
            DeleteKeyAndCheck(_mainTestKey);
        }

        [Fact]
        public void Create_Get_Delete_DateTime_Key()
        {
            var val = DateTime.Now;
            base._fRedis.SetKey(_mainTestKey, val, 1);
            var result = base._fRedis.Get<DateTime>(_mainTestKey, DateTime.Now);
            Assert.Equal(val, result);
            DeleteKeyAndCheck(_mainTestKey);
        }

        [Fact]
        public void Create_GetWithDefaultValue_Delete_DateTime_Key()
        {
            var val = new DateTime(1964, 12, 1);
            DeleteKeyAndCheck(_mainTestKey);
            var result = base._fRedis.Get<DateTime>(_mainTestKey, val);
            Assert.Equal(val, result);
        }

        [Fact]
        public void Create_Get_List_Key_String()
        {
            var list = new List<string>() { "A", "B", "C" };
            base._fRedis.SetList<string>(_mainTestKey, list);
            var result = base._fRedis.GetList<string>(_mainTestKey);
            Assert.Equal(list, result);
        }

        [Fact]
        public void Create_Get_List_Key_Int()
        {
            var list = new List<int>() { 1, 2, 3 };
            base._fRedis.SetList<int>(_mainTestKey, list);
            var result = base._fRedis.GetList<int>(_mainTestKey);
            Assert.Equal(list, result);
        }

        [Fact]
        public void Create_Get_List_Key_Double()
        {
            var list = new List<double>() { 1.1, 2.2, 3.3 };
            base._fRedis.SetList<double>(_mainTestKey, list);
            var result = base._fRedis.GetList<double>(_mainTestKey);
            Assert.Equal(list, result);
        }

        [Fact]
        public void Create_Get_List_Key_Decimal()
        {
            var list = new List<decimal>() { 1.1M, 2.2M, 3.3M };
            base._fRedis.SetList<decimal>(_mainTestKey, list);
            var result = base._fRedis.GetList<decimal>(_mainTestKey);
            Assert.Equal(list, result);
        }


        [Fact]
        public void List_As_Queue_Push_Pop()
        {
            var list = new List<string>() { "A", "B", "C" };
            base._fRedis.SetList<string>(_mainTestKey, list);
            base._fRedis.ListAdd<string>(_mainTestKey, "D");

            var expectedList = new List<string>() { "A", "B", "C", "D" };
            var result = base._fRedis.GetList<string>(_mainTestKey);
            Assert.Equal(expectedList, result);

            var poped = base._fRedis.ListPop<string>(_mainTestKey);
            Assert.Equal("D", poped);

            expectedList = new List<string>() { "A", "B", "C"};
            result = base._fRedis.GetList<string>(_mainTestKey);
            Assert.Equal(expectedList, result);
        }

        [Fact]
        public void List_As_Queue_Insert_Remove()
        {
            var list = new List<string>() { "A", "B", "C" };
            base._fRedis.SetList<string>(_mainTestKey, list);

            base._fRedis.ListInsert<string>(_mainTestKey, "AA", 1);

            var expectedList = new List<string>() { "A", "AA", "B", "C"};
            var result = base._fRedis.GetList<string>(_mainTestKey);
            Assert.Equal(expectedList, result);

            var removedItem = base._fRedis.Remove<string>(_mainTestKey, 0);
            Assert.Equal("A", removedItem);
            removedItem = base._fRedis.Remove<string>(_mainTestKey, 0);
            Assert.Equal("AA", removedItem);
            removedItem = base._fRedis.Remove<string>(_mainTestKey, 1);
            Assert.Equal("C", removedItem);

            expectedList = new List<string>() { "B"};
            result = base._fRedis.GetList<string>(_mainTestKey);
            Assert.Equal(expectedList, result);
        }


        [Fact]
        public void List_As_Queue_Enqueue_Dequeue()
        {
            var queue = new List<string>() { };
            base._fRedis.SetList<string>(_mainTestKey, queue);
            base._fRedis.ListEnQueue<string>(_mainTestKey, "A");
            base._fRedis.ListEnQueue<string>(_mainTestKey, "B");
            base._fRedis.ListEnQueue<string>(_mainTestKey, "C");
            base._fRedis.ListEnQueue<string>(_mainTestKey, "D");

            var expectedQueue = new List<string>() { "A", "B", "C", "D"};
            var result = base._fRedis.GetList<string>(_mainTestKey);
            Assert.Equal(expectedQueue, result);

            var poped = base._fRedis.ListDeQueue<string>(_mainTestKey);
            Assert.Equal("A", poped);

            poped = base._fRedis.ListDeQueue<string>(_mainTestKey);
            Assert.Equal("B", poped);

            expectedQueue = new List<string>() { "C", "D"};
            result = base._fRedis.GetList<string>(_mainTestKey);
            Assert.Equal(expectedQueue, result);
        }

        [Fact]
        public void Create_Get_Dictionary_Key_Int()
        {
            var dic = new Dictionary<string, int>() { ["A"] = 1, ["B"] = 2, ["C"] = 3 };
            base._fRedis.SetDictionary<int>(_mainTestKey, dic);
            var result = base._fRedis.GetDictionary<int>(_mainTestKey);
            Assert.Equal(dic, result);
        }

        [Fact]
        public void Create_Get_Dictionary_Key_String()
        {
            var dic = new Dictionary<string, string>() { ["A"] = "a", ["B"] = "b", ["C"] = "c" };
            base._fRedis.SetDictionary<string>(_mainTestKey, dic);
            var result = base._fRedis.GetDictionary<string>(_mainTestKey);
            Assert.Equal(dic, result);
        }

        [Fact]
        public void Create_Get_GetDictionaryItem_Key_String()
        {
            var dic = new Dictionary<string, string>() { ["A"] = "a", ["B"] = "b", ["C"] = "c" };
            base._fRedis.SetDictionary<string>(_mainTestKey, dic);
            var result = base._fRedis.GetDictionaryItem<string>(_mainTestKey, "B");
            Assert.Equal("b", result);
        }


        [Fact]
        public void Create_Get_SetDictionaryItemKey_Key_String()
        {
            var dic = new Dictionary<string, string>() { ["A"] = "a", ["B"] = "b", ["C"] = "c" };
            base._fRedis.SetDictionary<string>(_mainTestKey, dic);
            var result = base._fRedis.GetDictionaryItem<string>(_mainTestKey, "B");
            Assert.Equal("b", result);

            const string newValue = "z";
            base._fRedis.SetDictionaryItemKey<string>(_mainTestKey, "B", newValue);

            result = base._fRedis.GetDictionaryItem<string>(_mainTestKey, "B");
            Assert.Equal(newValue, result);
        }

        [Fact]
        public void GetKeys()
        {
            // var r = base._fRedis.DeleteKeys(base._fRedis.GetKeys($"{multiTestKeyPrefix}*"));

            var dataList = DS.Range(1, 10 + 1, 1);
            CreateIntegerSetOfKeys(dataList);

            var result = base._fRedis.GetKeys($"{multiTestKeyPrefix}*");
            Assert.Equal(10, result.Count);
            var sb = new System.Text.StringBuilder(512);

            foreach (var i in result)
            {
                switch (i.Type)
                {
                    case StackExchange.Redis.RedisType.String:
                        sb.Append(base._fRedis.Get<string>(i.Key)).Append(", ");
                        break;
                }
            }


            var expected = string.Join(", ", dataList);
            Assert.Equal(expected, sb.ToString().RemoveLastChar().RemoveLastChar());

            dataList.ForEach(intVal =>
            {
                var key = $"{multiTestKeyPrefix}{intVal:000}";
                DeleteKeyAndCheck(key);
            });
        }

        private void CreateIntegerSetOfKeys(List<int> dataList)
        {
            dataList.ForEach(intVal =>
            {
                var key = $"{multiTestKeyPrefix}{intVal:000}";
                DeleteKeyAndCheck(key);
                base._fRedis.SetKey(key, intVal);
            });
        }

        [Fact]
        public void DeleteKeys_FromQuery()
        {
            // var r = base._fRedis.DeleteKeys(base._fRedis.GetKeys($"{multiTestKeyPrefix}*"));

            var dataList = DS.Range(1, 10 + 1, 1);
            CreateIntegerSetOfKeys(dataList);

            var result = base._fRedis.GetKeys($"{multiTestKeyPrefix}*");
            Assert.Equal(10, result.Count);

            base._fRedis.DeleteKeys(result);

            Thread.Sleep(1000*2); // Ensure keys are not cached

            result = base._fRedis.GetKeys($"{multiTestKeyPrefix}*");
            Assert.Equal(0, result.Count);
        }


        [Fact]
        public void DeleteKeys_FromListOfLeys()
        {
            var dataList = DS.Range(1, 10 + 1, 1);
            CreateIntegerSetOfKeys(dataList);

            var result = base._fRedis.GetKeys($"{multiTestKeyPrefix}*");
            Assert.Equal(10, result.Count);

            var keysToDelete = result.Select(i => i.Key).ToList();

            base._fRedis.DeleteKeys(keysToDelete);

            Thread.Sleep(1000*2); // Ensure keys are not cached

            result = base._fRedis.GetKeys($"{multiTestKeyPrefix}*");
            Assert.Equal(0, result.Count);
        }
    }
}