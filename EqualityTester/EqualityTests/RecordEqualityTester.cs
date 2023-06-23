using Equality.RecordHelpers;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static EqualityTester.EqualityTests.BasicEqualityTester;

namespace EqualityTester.EqualityTests
{
    public class RecordEqualityTester
    {
        public record SimpleRecordObject
        {
            public int Id { get; set; }
        }

        [Fact]
        public void TestRecordObjectEquals()
        {
            var item1 = new SimpleRecordObject { Id = 1 };
            var item2 = new SimpleRecordObject { Id = 1 };
            (item1 == item2).ShouldBe(true);
            (item1 != item2).ShouldBe(false);
            (item1.GetHashCode() == item2.GetHashCode()).ShouldBe(true);
            item1.Equals(item2).ShouldBe(true);
        }

        public record RecordWithNestedRecord
        {
            public int Id { get; set; }
            public SimpleRecordObject SimpleRecordObject { get; set; }
        }

        [Fact]
        public void TestNestedRecord()
        {
            var nested1 = new RecordWithNestedRecord { Id = 1, SimpleRecordObject = new SimpleRecordObject { Id = 2 } };
            var nested2 = new RecordWithNestedRecord { Id = 1, SimpleRecordObject = new SimpleRecordObject { Id = 2 } };
            (nested1 == nested2).ShouldBe(true);
            (nested1 != nested2).ShouldBe(false);
            nested1.GetHashCode().ShouldBe(nested2.GetHashCode());
            nested1.Equals(nested2).ShouldBe(true);
        }

        [Fact]
        public void Test_Compare_List_Of_Record_Object_Equals()
        {
            var list1 = new List<SimpleRecordObject> { new SimpleRecordObject { Id = 1 }, new SimpleRecordObject { Id = 2 } };
            var list2 = new List<SimpleRecordObject> { new SimpleRecordObject { Id = 1 }, new SimpleRecordObject { Id = 2 } };
            var list3 = new List<SimpleRecordObject> { new SimpleRecordObject { Id = 2 }, new SimpleRecordObject { Id = 1 } };
            var list4 = new List<SimpleRecordObject> { new SimpleRecordObject { Id = 1 }, new SimpleRecordObject { Id = 3 },
                                                        new SimpleRecordObject { Id = 4 }, new SimpleRecordObject { Id = 5 } };

            // can't compare lists of records directly for equality
            (list1 == list2).ShouldBe(false);

            // but you can compare the contents because they are records
            list1.Except(list2).Count().ShouldBe(0);
            list2.Except(list1).Count().ShouldBe(0);
            // and order doesn't matter            
            list2.Except(list3).Count().ShouldBe(0);
            list3.Except(list2).Count().ShouldBe(0);

            // and we retrieved the items in list4 that were not in list 3
            var differences = list4.Except(list3);
            differences.Count().ShouldBe(3);
            // and they were left in order!
            differences.First().Id.ShouldBe(3);
            differences.Last().Id.ShouldBe(5);
        }

        public record RecordWithNestedClass
        {
            public int Id { get; set; }
            public SimpleClassObject SimpleObject { get; set; } = new();
        }

        [Fact]
        public void Test_Record_With_Nested_Class_Equals_Fails()
        {
            var record1 = new RecordWithNestedClass { Id = 1, SimpleObject = new SimpleClassObject { Id = 1 } };
            var record2 = new RecordWithNestedClass { Id = 1, SimpleObject = new SimpleClassObject { Id = 1 } };
            (record1 == record2).ShouldBe(false);
        }

        public record RecordWithListOfSimpleRecordObjects
        {
            public int Id { get; set; }
            public List<SimpleRecordObject> SimpleRecordObjects { get; set; } = new();
        }

        [Fact]
        public void Test_Record_With_List_Equals_Fails()
        {
            // but it can't deal with a record with a list of records
            var list1 = new List<RecordWithListOfSimpleRecordObjects> { new RecordWithListOfSimpleRecordObjects
                { Id = 1, SimpleRecordObjects = new List<SimpleRecordObject> { new SimpleRecordObject() } } };
            var list2 = new List<RecordWithListOfSimpleRecordObjects> { new RecordWithListOfSimpleRecordObjects
                { Id = 1, SimpleRecordObjects = new List<SimpleRecordObject> { new SimpleRecordObject() } } };
            // the list comparison fails
            list2.Except(list1).Count().ShouldBe(1);
        }

        public record RecordWithRecordEqualityList
        {
            public int Id { get; set; }
            public RecordEqualityList<SimpleRecordObject> SimpleRecordObjects { get; set; }
            public RecordWithRecordEqualityList()
            {
                SimpleRecordObjects = new RecordEqualityList<SimpleRecordObject>();
            }
        }

        [Fact]
        public void Test_ListsRecordObjects_WithRecordEqualityLists_Equals()
        {
            // but it can deal with a RecordEqualitylist of records
            var reqList1 = new RecordEqualityList<RecordWithRecordEqualityList> { new RecordWithRecordEqualityList { Id = 1, SimpleRecordObjects =
                            new RecordEqualityList<SimpleRecordObject> { new SimpleRecordObject { Id = 2 }, new SimpleRecordObject { Id = 3 } } },
                    new RecordWithRecordEqualityList { Id = 2, SimpleRecordObjects =
                            new RecordEqualityList<SimpleRecordObject> { new SimpleRecordObject { Id = 4 }, new SimpleRecordObject { Id = 5 } } }
            };

            var reqList2 = new RecordEqualityList<RecordWithRecordEqualityList> { new RecordWithRecordEqualityList { Id = 1, SimpleRecordObjects =
                            new RecordEqualityList<SimpleRecordObject> { new SimpleRecordObject { Id = 2 }, new SimpleRecordObject { Id = 3 } } },
                    new RecordWithRecordEqualityList { Id = 2, SimpleRecordObjects =
                            new RecordEqualityList<SimpleRecordObject> { new SimpleRecordObject { Id = 4 }, new SimpleRecordObject { Id = 5 } } }
            };

            reqList1.Equals(reqList2).ShouldBe(true);
            reqList1.GetHashCode().ShouldBeEquivalentTo(reqList2.GetHashCode());
            (reqList1 == reqList2).ShouldBe(true);
            (reqList1 != reqList2).ShouldBe(false);
            reqList2.Except(reqList1).Count().ShouldBe(0);
        }

        [Fact]
        public void TestRecordEqualityList_of_RecordObjectWithRecordEqualityListEquals()
        {
            // but it can deal with a RecordEqualitylist of records
            var list1 = new RecordEqualityList<RecordWithRecordEqualityList> { new RecordWithRecordEqualityList { Id = 1, SimpleRecordObjects = new RecordEqualityList<SimpleRecordObject> { new SimpleRecordObject() } } };
            var list2 = new RecordEqualityList<RecordWithRecordEqualityList> { new RecordWithRecordEqualityList { Id = 1, SimpleRecordObjects = new RecordEqualityList<SimpleRecordObject> { new SimpleRecordObject() } } };

            list2.Except(list1).Count().ShouldBe(0);
        }

        public record RecordWithSpan
        {
            public TimeSpan Span { get; set; }
        }

        [Fact]
        public void Test_TimeSpan()
        {
            var list1 = new RecordEqualityList<RecordWithSpan> { new RecordWithSpan { Span = new TimeSpan(1, 1, 1) } };
            var list2 = new RecordEqualityList<RecordWithSpan> { new RecordWithSpan { Span = new TimeSpan(1, 1, 1) } };
            (list1 == list2).ShouldBe(true);
        }
    }
}
