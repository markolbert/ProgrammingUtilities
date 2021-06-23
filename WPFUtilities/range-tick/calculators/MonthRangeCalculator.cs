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
    public class MonthRangeCalculator : RangeCalculator<DateTime>
    {
        public MonthRangeCalculator(
            IJ4JLogger? logger
        )
            : base(TickStyle.Date, logger)
        {
        }

        protected override decimal GetMinorTicksInRange(DateTime minValue, DateTime maxValue, decimal minorTickWidth)
        {
            var firstMonth = GetMonthNumber( minValue );
            var lastMonth = GetMonthNumber( maxValue );

            var range = lastMonth - firstMonth;

            if( range == 0 )
                return 1;

            return range / minorTickWidth;
        }

        protected override decimal GetPowerOfTen( DateTime minValue, DateTime maxValue, int minTickPowerOfTen )
        {
            var firstMonth = GetMonthNumber( minValue );
            var lastMonth = GetMonthNumber( maxValue );

            var range = lastMonth - firstMonth;

            if( range == 0 )
                return 1;

            var scalingExponent = (int) ( Math.Log10( range ) - minTickPowerOfTen );
            scalingExponent = scalingExponent < 0 ? 0 : scalingExponent;

            return (decimal) Math.Pow( 10, scalingExponent );
        }

        protected override bool GetAdjustedEndPoint( DateTime toAdjust, decimal minorTickWidth, EndPoint endPoint, out DateTime result )
        {
            result = toAdjust;

            var monthNum = GetMonthNumber( toAdjust );

            switch (endPoint)
            {
                case EndPoint.StartOfRange:
                    var startModulo = ( monthNum - 1 ) % minorTickWidth;

                    result = startModulo switch
                    {
                        0 => new DateTime( toAdjust.Year, toAdjust.Month, 1 ),
                        _ => GetFirstDay( toAdjust, -startModulo )
                    };

                    return true;

                case EndPoint.EndOfRange:
                    var endModulo = monthNum % minorTickWidth;

                    result = endModulo switch
                    {
                        0 => GetFirstDay( toAdjust, endModulo + 1 ),
                        _ => GetFirstDay( toAdjust, minorTickWidth - endModulo + 1 )
                    };

                    return true;

                default:
                    Logger?.Error("Unsupported EndPoint value {0}", endPoint);
                    return false;
            }
        }

        public static int GetMonthNumber( DateTime dt ) => dt.Year * 12 + dt.Month;

        public static DateTime GetFirstDay( DateTime dt, decimal delta )
        {
            var deltaYears = (int) delta / 12;
            var deltaMonths = (int) ( delta - 12 * deltaYears );

            var year = dt.Year + deltaYears;
            var month = dt.Month + deltaMonths;

            if( month <= 12 ) return new DateTime( year, month, 1 );

            month -= 12;
            year++;

            return new DateTime( year, month, 1 );
        }
    }
}