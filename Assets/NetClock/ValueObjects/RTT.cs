using System;
using System.Globalization;

namespace NetClock
{
    public readonly struct RTT
    {
        public readonly long Ticks;

        public RTT(long ticks)
        {
            Ticks = ticks;
        }

        public static implicit operator long(RTT rtt)
        {
            return rtt.Ticks;
        }

        public static implicit operator RTT(TimeSpan timeSpan)
        {
            return new RTT(timeSpan.Ticks);
        }

        public override string ToString()
        {
            return TimeSpan.FromTicks(Ticks).TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
        }
    }
}