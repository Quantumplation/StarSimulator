using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarSimulator
{
    public class Rate : IComparable<Rate>, IEquatable<Rate>
    {
        public static Rate Zero() { return new Rate { PerTick = 0 }; }

        public static Rate FromString(string val) { return new Rate { PerTick = decimal.Parse(val) }; }

        internal decimal PerTick { get; set; }

        public int CompareTo(Rate other)
        {
            return PerTick.CompareTo(other.PerTick);
        }

        public bool Equals(Rate other)
        {
            return PerTick.Equals(other.PerTick);
        }

        public static bool operator <(Rate left, Rate right)
        {
            return left.PerTick < right.PerTick;
        }
        public static bool operator >(Rate left, Rate right)
        {
            return left.PerTick > right.PerTick;
        }
        public static bool operator <=(Rate left, Rate right)
        {
            return left.PerTick <= right.PerTick;
        }
        public static bool operator >=(Rate left, Rate right)
        {
            return left.PerTick >= right.PerTick;
        }

        public static decimal operator *(Rate left, Duration right)
        {
            return left.PerTick * right.Ticks;
        }
        public static decimal operator *(Duration left, Rate right)
        {
            return right.PerTick * left.Ticks;
        }

        public static Rate operator *(Rate left, decimal multiple)
        {
            return new Rate { PerTick = left.PerTick * multiple };
        }
        public static Rate operator /(Rate left, decimal multiple)
        {
            return new Rate { PerTick = left.PerTick / multiple };
        }
        public static Rate operator +(Rate left, Rate right)
        {
            return new Rate { PerTick = left.PerTick + right.PerTick };
        }
        public static Rate operator -(Rate left, Rate right)
        {
            return new Rate { PerTick = left.PerTick - right.PerTick };
        }
    }
}
