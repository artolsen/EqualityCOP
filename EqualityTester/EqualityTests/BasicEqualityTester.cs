using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EqualityTester.EqualityTests
{
    public class BasicEqualityTester
    {
        public void TestValueEquals()
        {
            var a = true;
            a.ShouldBe(true);

            var num = 1;
            num.ShouldBe(1);
        }

        public class SimpleClassObject
        {
            public int Id { get; set; }
        }

        [Fact]
        public void TestClassObjectEquals()
        {
            // without overloading the equals and hashcode of the object class equality check fails,
            // this is an object reference equals and does not compare values
            var item1 = new SimpleClassObject { Id = 1 };
            var item2 = new SimpleClassObject { Id = 1 };
            (item1 == item2).ShouldBe(false);
        }

        public class SimpleOverRideEqualsClassObject
        {
            public int Id { get; set; }

            public override bool Equals(object? obj)
            {
                if (obj == null) return false;
                if (!(obj is SimpleOverRideEqualsClassObject)) return false;                
                return this.Id == ((SimpleOverRideEqualsClassObject)obj).Id; // this may need to be added to as needed as the object evolves
            }
            public override int GetHashCode()
            {                
                return Id.GetHashCode(); // this may need to be added to as needed as the object evolves
            }

            public static bool operator ==(SimpleOverRideEqualsClassObject so1, SimpleOverRideEqualsClassObject so2)
            {
                if ((object)so1 == null)
                {
                    return (object)so2 == null;
                }
                return so1.Equals(so2);
            }

            public static bool operator !=(SimpleOverRideEqualsClassObject so1, SimpleOverRideEqualsClassObject so2)
            {
                return !(so1 == so2);
            }
        }

        [Fact]
        public void Test_Override_Equals_Class_Object()
        {
            var item1 = new SimpleOverRideEqualsClassObject { Id = 1 };
            var item2 = new SimpleOverRideEqualsClassObject { Id = 1 };
            item1.Equals(item2).ShouldBeTrue();
            (item1 == item2).ShouldBeTrue();
            (item1 != item2).ShouldBeFalse();
            item1.GetHashCode().ShouldBe(item2.GetHashCode());
        }
    }
}
