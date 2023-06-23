using Equality.RecordHelpers;
using Shouldly;
using System.Text.Json;
using Xunit;

namespace EqualityTester.EqualityTests
{
    public class RecordEqualityListSerializationTester
    {
        private const string BasePath = @"..\..\..\Files\";

        public record BaseRecord
        {
            public string BaseName { get; set; }
            public int BaseId { get; set; }
            public RecordEqualityList<SubRecord> SubRecords { get; set; }

            public BaseRecord()
            {
                SubRecords = new RecordEqualityList<SubRecord>();
            }
        }

        public record SubRecord
        {
            public string SubRecordName { get; set; }
            public int SubRecordId { get; set; }
        }

        [Fact]
        public void TestDeserialize()
        {
            var jsonString1 = File.ReadAllText(Path.Combine(BasePath, "BasePayload1.json"));
            var data = JsonSerializer.Deserialize<List<BaseRecord>>(jsonString1);
            data.Count.ShouldBe(4);
            data[0].SubRecords.Count.ShouldBe(3);
        }

        [Fact]
        public void TestSerialize()
        {
            var baseList = new List<BaseRecord>();
            var equalityList = new RecordEqualityList<BaseRecord>();
            var rec = new BaseRecord { BaseId = 1, BaseName = "name" };
            baseList.Add(rec);
            equalityList.Add(rec);

            var baseListString = JsonSerializer.Serialize(baseList);
            var equalityString = JsonSerializer.Serialize(equalityList);

            // RecordEqualityList should serialize to a plain json list
            baseListString.ShouldBe(equalityString);
        }

        [Fact]
        public void TestWithDeserializedData()
        {
            var data1 = JsonSerializer.Deserialize<List<BaseRecord>>(File.ReadAllText(Path.Combine(BasePath, "BasePayload1.json")));
            var data2 = JsonSerializer.Deserialize<List<BaseRecord>>(File.ReadAllText(Path.Combine(BasePath, "BasePayload2.json")));

            var differencesInSet2 = data2.Except(data1).OrderBy(o => o.BaseId).ToList(); // should return objects from data2 that are not equal to the ones on data1

            differencesInSet2.Count.ShouldBe(2);
            differencesInSet2[0].BaseId.ShouldBe(1);
            differencesInSet2[0].BaseName.ShouldBe("BaseRecord1DifferentName");
            differencesInSet2[1].SubRecords[1].SubRecordName.ShouldBe("SubRecord2DifferentName");
            differencesInSet2[1].SubRecords[1].SubRecordId.ShouldBe(22);

            var differencesInSet1 = data1.Except(data2).OrderBy(o => o.BaseId).ToList(); // should return objects from data1 that are not equal or represented to the ones on data2
            differencesInSet1.Count.ShouldBe(3);
            differencesInSet1[0].BaseId.ShouldBe(1);
            differencesInSet1[0].BaseName.ShouldBe("BaseRecord1");
            differencesInSet1[1].SubRecords[1].SubRecordName.ShouldBe("SubRecord2");
            differencesInSet1[1].SubRecords[1].SubRecordId.ShouldBe(22);
            differencesInSet1[2].BaseName.ShouldBe("BaseRecord3");
            differencesInSet1[2].SubRecords.Count.ShouldBe(3);
        }

        [Fact]
        public void TestDeserializeEmptyNodeEqualsNodeNotPresent()
        {
            var emptyNodeObject = JsonSerializer.Deserialize<List<BaseRecord>>(File.ReadAllText(Path.Combine(BasePath, "BasePayloadEmptySubRecordNode.json")));

            var nullNodeObject = JsonSerializer.Deserialize<List<BaseRecord>>(File.ReadAllText(Path.Combine(BasePath, "BasePayloadNoSubRecordNode.json")));

            emptyNodeObject.Except(nullNodeObject).Count().ShouldBe(0);
        }
    }
}
