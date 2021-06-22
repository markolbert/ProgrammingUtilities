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
    public class IntegerRangeCalculator : RangeCalculator<int>
    {
        public IntegerRangeCalculator(
            IJ4JLogger? logger
        )
            : base(TickStyle.Numeric, logger)
        {
        }

        protected override decimal GetMinorTicksInRange(int minValue, int maxValue, decimal minorTickWidth)
        {
            var adjRange = maxValue - minValue;

            return adjRange / minorTickWidth;
        }

        protected override decimal GetPowerOfTen(int minValue, int maxValue, int minTickPowerOfTen)
        {
            var range = maxValue - minValue;

            if (range == 0)
                return 1;

            var scalingExponent = (int)(Math.Log10((double)range) - minTickPowerOfTen);
            scalingExponent = scalingExponent < 0 ? 0 : scalingExponent;

            return (decimal)Math.Pow(10, scalingExponent);
        }

        protected override bool GetAdjustedEndPoint(int toAdjust, decimal minorTickWidth, EndPoint endPoint, out int result )
        {
            result = toAdjust;

            var modulo = toAdjust % minorTickWidth;

            if( modulo == 0 )
                return true;

            var rounding = (int) ( minorTickWidth - Math.Abs( modulo ) );

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