using Equality.RecordHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqualityTester.Models
{
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
}
