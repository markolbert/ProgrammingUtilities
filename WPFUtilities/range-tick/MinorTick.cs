using System;

namespace J4JSoftware.WPFUtilities
{
    public record MinorTick( int NormalizedSize, int NumberPerMajor );

    public record ScaledMinorTick( int NormalizedSize, int PowerOfTen, int NumberPerMajor )
        : MinorTick( NormalizedSize, NumberPerMajor )
    {
        public ScaledMinorTick( MinorTick minorTick, int powerOfTen )
            : this( minorTick.NormalizedSize, powerOfTen, minorTick.NumberPerMajor )
        {
        }

        public double Size => NormalizedSize * Math.Pow( 10, PowerOfTen );
    }
}