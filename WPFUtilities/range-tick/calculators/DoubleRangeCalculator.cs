#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'WPFUtilities' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using J4JSoftware.Logging;

namespace J4JSoftware.WPFUtilities
{
    public class DoubleRangeCalculator : RangeCalculator<double>
    {
        public DoubleRangeCalculator(
            IJ4JLogger? logger
        )
            : base(TickStyle.Numeric, logger)
        {
        }

        protected override decimal GetMinorTicksInRange(double minValue, double maxValue, decimal minorTickWidth)
        {
            var rawRange = maxValue - minValue;

            if( rawRange == 0 )
                return 1;

            decimal range = 0;

            try
            {
                range = Convert.ToDecimal( rawRange );
            }
            catch
            {
                Logger?.Fatal("Range ({0}) too large to convert to decimal", rawRange);
                throw new OverflowException( $"Range ({rawRange}) too large to convert to decimal" );
            }

            return range / minorTickWidth;
        }

        protected override decimal GetPowerOfTen(double minValue, double maxValue, int minTickPowerOfTen)
        {
            var range = maxValue - minValue;

            if (range == 0)
                return 1;

            var scalingExponent = (int)(Math.Log10((double)range) - minTickPowerOfTen);
            scalingExponent = scalingExponent < 0 ? 0 : scalingExponent;

            decimal retVal = 0;

            try
            {
                retVal = Convert.ToDecimal( Math.Pow( 10, scalingExponent ) );
            }
            catch
            {
                Logger?.Fatal("Scaling exponent ({0}) too large to convert to decimal power of ten", scalingExponent);
                throw new OverflowException($"Scaling exponent ({scalingExponent}) too large to convert to decimal power of ten");
            }

            return retVal;
        }

        protected override bool GetAdjustedEndPoint(double toAdjust, decimal minorTickWidth, EndPoint endPoint, out double result )
        {
            result = toAdjust;

            decimal decToAdjust = 0;

            try
            {
                decToAdjust = Convert.ToDecimal( Math.Abs( toAdjust ) );
            }
            catch
            {
                Logger?.Error("Value ({0}) too large to convert to decimal", toAdjust);
                return false;
            }

            var modulo = decToAdjust % minorTickWidth;

            if (modulo == 0)
                return true;

            var rounding = (double) ( minorTickWidth - Math.Abs( modulo ) );

            switch (endPoint)
            {
                case EndPoint.StartOfRange:
                    result = toAdjust < 0 ? toAdjust - rounding : toAdjust + rounding;
                    return true;

                case EndPoint.EndOfRange:
                    result = toAdjust + rounding;
                    return true;

                default:
                    Logger?.Error("Unsupported EndPoint value {0}", endPoint);
                    return false;
            }
        }
    }
}