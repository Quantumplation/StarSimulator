using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarSimulator
{
    public class Timestamp : IComparable<Timestamp>, IEquatable<Timestamp>
    {
        static Timestamp() { TimeManager = new TimeManager(); }
        public static TimeManager TimeManager { get; set; }
        public static Timestamp Now() { return new Timestamp { Ticks = TimeManager.CurrentTick }; }
        public static Timestamp Min() { return new Timestamp { Ticks = 0 }; }
        public static Timestamp Max() { return new Timestamp { Ticks = long.MaxValue }; }

        public static Timestamp FromString(string val) { return new Timestamp { Ticks = long.Parse(val) }; }

        internal long Ticks { get; set; }

        public int CompareTo(Timestamp other)
        {
            return Ticks.CompareTo(other.Ticks);
        }

        public bool Equals(Timestamp other)
        {
            return Ticks.Equals(other.Ticks);
        }

        public override int GetHashCode()
        {
            return Ticks.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if(obj is Timestamp)
                return Equals(obj as Timestamp);
            return false;
        }

        public static bool operator ==(Timestamp left, Timestamp right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Timestamp left, Timestamp right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Timestamp left, Timestamp right)
        {
            return left.Ticks < right.Ticks;
        }
        public static bool operator >(Timestamp left, Timestamp right)
        {
            return left.Ticks > right.Ticks;
        }
        public static bool operator <=(Timestamp left, Timestamp right)
        {
            return left.Ticks <= right.Ticks;
        }
        public static bool operator >=(Timestamp left, Timestamp right)
        {
            return left.Ticks >= right.Ticks;
        }

        public static Timestamp operator +(Timestamp left, Duration right)
        {
            return new Timestamp { Ticks = left.Ticks + right.Ticks };
        }

        public static Timestamp operator -(Timestamp left, Duration right)
        {
            return new Timestamp { Ticks = left.Ticks - right.Ticks };
        }

        public static Duration operator -(Timestamp left, Timestamp right)
        {
            return new Duration { Ticks = left.Ticks - right.Ticks };
        }
    }

    public class Duration : IComparable<Duration>, IEquatable<Duration>
    {
        public static Duration Zero() { return new Duration { Ticks = 0 }; }
        public static Duration Epsilon() { return new Duration { Ticks = 1 }; }

        internal long Ticks { get; set; }

        public int CompareTo(Duration other)
        {
            return Ticks.CompareTo(other.Ticks);
        }

        public bool Equals(Duration other)
        {
            return Ticks.Equals(other.Ticks);
        }

        public Duration Abs()
        {
            return new Duration { Ticks = Math.Abs(Ticks) };
        }

        public static bool operator ==(Duration left, Duration right)
        {
            return left.Equals(right);
        }

        public override int GetHashCode()
        {
            return Ticks.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Duration)) return false;
            return this.Equals(obj as Duration);
        }

        public static bool operator !=(Duration left, Duration right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Duration left, Duration right)
        {
            return left.Ticks < right.Ticks;
        }
        public static bool operator >(Duration left, Duration right)
        {
            return left.Ticks > right.Ticks;
        }
        public static bool operator <=(Duration left, Duration right)
        {
            return left.Ticks <= right.Ticks;
        }
        public static bool operator >=(Duration left, Duration right)
        {
            return left.Ticks >= right.Ticks;
        }

        public static Duration operator +(Duration left, Duration right)
        {
            return new Duration { Ticks = left.Ticks + right.Ticks };
        }

        public static Timestamp operator +(Duration left, Timestamp right)
        {
            return right + left;
        }

        public static Duration operator *(Duration left, double multiple)
        {
            return new Duration { Ticks = (long)(left.Ticks * multiple) };
        }
        public static Duration operator *(double multiple, Duration left)
        {
            return new Duration { Ticks = (long)(left.Ticks * multiple) };
        }
        public static Duration operator /(Duration left, double multiple)
        {
            return new Duration { Ticks = (long)(left.Ticks / multiple) };
        }


        public static Rate operator /(decimal value, Duration d)
        {
            return new Rate()
            {
                PerTick = value / d.Ticks,
            };
        }
    }
}
