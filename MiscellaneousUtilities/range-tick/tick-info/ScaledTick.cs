using System;

namespace J4JSoftware.Utilities
{
    public record ScaledTick : Tick
    {
        public ScaledTick()
        {
        }

        public ScaledTick( ScaledTick tick, int powerOfTen )
        {
            NormalizedSize = tick.NormalizedSize;
            NumberPerMajor = tick.NumberPerMajor;
            PowerOfTen = powerOfTen;
        }


        public int PowerOfTen { get; init; }

        public override double Size => base.Size * Math.Pow( 10, PowerOfTen );
    }
}