using System;

namespace J4JSoftware.WPFUtilities
{
    public record DoubleTick : ScaledTick
    {
        public DoubleTick()
        {
        }

        public DoubleTick( Tick tick, int powerOfTen )
        {
            NormalizedSize = tick.NormalizedSize;
            NumberPerMajor = tick.NumberPerMajor;
            PowerOfTen = powerOfTen;
        }
    }
}