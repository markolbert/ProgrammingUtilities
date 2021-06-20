using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.WPFUtilities
{
    public class RangeTickManager
    {
        public RangeTickManager( 
            int minTickPowerOfTen,
            params MinorTickInfo[] minorTickInfo 
            )
        {
            MinimumTickPowerOfTen = minTickPowerOfTen;
            
            MinorTickChoices = minorTickInfo.Length == 0 ? MinorTickInfo.Default : minorTickInfo;
        }

        public int MinimumTickPowerOfTen { get; }
        public MinorTickInfo[] MinorTickChoices { get; }

        public List<RangeTickParameters> Calculate( double minValue, double maxValue )
        {
            var rawRange = Math.Abs( maxValue - minValue );

            var scalingExponent = (int) ( Math.Log10( rawRange ) - MinimumTickPowerOfTen );
            var powerOfTen = Math.Pow( 10, scalingExponent );

            var retVal = new List<RangeTickParameters>();

            foreach( var mtChoice in MinorTickChoices )
            {
                var adjMinValue = GetAdjustedEndPoint( minValue, mtChoice, powerOfTen );
                var adjMaxValue = GetAdjustedEndPoint( maxValue, mtChoice, powerOfTen );

                var adjRange = adjMaxValue - adjMinValue;
                var totalMinorTicks = adjRange / ( mtChoice.NormalizedTickWidth * powerOfTen );

                var majorTicks = (int) totalMinorTicks / mtChoice.MinorTicksPerMajorTick;

                var modulo = totalMinorTicks % mtChoice.MinorTicksPerMajorTick;
                if ( modulo != 0 ) majorTicks++;

                retVal.Add( new RangeTickParameters(
                    majorTicks,
                    mtChoice.MinorTicksPerMajorTick * mtChoice.NormalizedTickWidth*powerOfTen,
                    mtChoice.MinorTicksPerMajorTick,
                    mtChoice.NormalizedTickWidth * powerOfTen,
                    adjMinValue,
                    adjMaxValue )
                );
            }

            return retVal;
        }

        private double GetAdjustedEndPoint( double toAdjust, MinorTickInfo mtChoice, double powerOfTen )
        {
            var absAdjust = Math.Abs( toAdjust );
            var mtWidth = mtChoice.NormalizedTickWidth * powerOfTen;
            var modulo = absAdjust % mtWidth;
            var rounding = mtWidth - modulo;
            absAdjust += rounding;

            return absAdjust * Math.Sign( toAdjust );
        }

        public int RoundUpInteger( double toRound )
            => (int) ( toRound < 0 ? Math.Floor( toRound ) : Math.Ceiling( toRound ) );
    }
}
