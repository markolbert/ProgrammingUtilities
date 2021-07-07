using System;
using System.Linq;
using Superpower.Model;

namespace J4JSoftware.WPFUtilities
{
    public record Tick
    {
        public uint NormalizedSize { get; init; }
        public uint NumberPerMajor { get; init; }

        public virtual double Size => NormalizedSize;
        public double MajorSize => Size * NumberPerMajor;

        public virtual double RoundUp(double toRound)
        {
            var scalingFactor = GetNormalizingFactor( toRound, Size );

            var modulo = ( toRound * scalingFactor ) % ( Size * scalingFactor );
            if (modulo == 0)
                return toRound;

            modulo /= scalingFactor;

            return toRound < 0 ? toRound - modulo : toRound + Size - modulo;
        }

        public virtual double RoundDown(double toRound)
        {
            var scalingFactor = GetNormalizingFactor(toRound, Size);

            var modulo = (toRound * scalingFactor) % (Size * scalingFactor);
            if (modulo == 0)
                return toRound;

            modulo /= scalingFactor;

            return toRound < 0 ? toRound - Size - modulo : toRound - modulo;
        }

        public virtual uint GetMinorTicksInRange(double minValue, double maxValue)
        {
            var range = maxValue - minValue;

            if (range == 0)
                return 1;

            return (uint) Math.Round( range / Size );
        }

        protected int GetNormalizingFactor( params double[] values )
        {
            var minExponent = values.Min( x => x == 0
                ? 0
                : (int) Math.Floor( Math.Log10( Math.Abs( x ) ) ) );

            if( minExponent >= 0 )
                return 1;

            var retVal = 1;

            for ( var idx = 0; idx < -minExponent; idx++ )
            {
                retVal *= 10;
            }

            return retVal;
        }
    }
}