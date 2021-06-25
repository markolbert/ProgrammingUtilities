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
using System.Collections.Generic;
using System.Windows.Navigation;
using J4JSoftware.Logging;

namespace J4JSoftware.WPFUtilities
{
    public class MonthRangeCalculator : RangeCalculator<DateTime>, IDateRangeCalculator
    {
        public MonthRangeCalculator(
            IJ4JLogger? logger
        )
            : base(logger)
        {
        }

        public DateRangeFocus Focus => DateRangeFocus.Month;

        protected override decimal GetMinorTicksInRange(DateTime minValue, DateTime maxValue, decimal minorTickWidth)
        {
            var firstMonth = GetMonthNumber( minValue );
            var lastMonth = GetMonthNumber( maxValue );

            var range = lastMonth - firstMonth;

            if( range == 0 )
                return 1;

            return range / minorTickWidth;
        }

        protected override RangeParameters<DateTime> GetDefaultRange( DateTime minValue, DateTime maxValue )
        {
            var numMonths = GetMonthNumber( maxValue ) - GetMonthNumber( minValue );

            var years = numMonths / 12;

            if( numMonths % 12 != 0 )
                years++;

            var decades = years / 10;
            if( years % 10 != 0 )
                decades++;

            var daysInLastMonth = DateTime.DaysInMonth( maxValue.Year, maxValue.Month );

            return numMonths switch
            {
                <= 60 => new RangeParameters<DateTime>(
                    years,
                    12,
                    1,
                    minValue.AddDays( 1 - minValue.Day ),
                    maxValue.AddDays( daysInLastMonth - maxValue.Day ) ),
                _ => new RangeParameters<DateTime>(
                    decades,
                    10,
                    12,
                    minValue.AddDays( 1 - minValue.Day ),
                    maxValue.AddDays( daysInLastMonth - maxValue.Day ) ),
            };
        }

        protected override int StartingExponent( int rawExponent ) => rawExponent > 2 ? rawExponent - 2 : 0;

        protected override TickStatus GetScalingFactors(
            int generation,
            DateTime minValue,
            DateTime maxValue,
            out List<TickInfo> result)
        {
            generation = generation < 0 ? 0 : generation;
            result = new List<TickInfo> { new TickInfo( 1, 1 ) };

            var minMonthNum = GetMonthNumber( minValue );
            var maxMonthNum = GetMonthNumber( maxValue );

            var numMonths = Math.Abs( maxMonthNum - minMonthNum );

            if (numMonths == 0)
                return TickStatus.ZeroRange;

            var numYears = numMonths / 12;
            if( numMonths % 12 != 0 )
                numYears++;

            var exponent = (int)Math.Log10(numYears);
            if( Math.Pow( 10, ( exponent + generation ) ) >= numMonths )
                return TickStatus.RangeExceeded;

            result = numYears switch
            {
                <= 10 => CreateTicks( exponent, generation, ( 1, 12 ), ( 3, 4 ), ( 6, 2 ) ),
                < 40 and > 10 => CreateTicks( exponent, generation, ( 3, 4 ), ( 6, 2 ),
                    ( 12, 10 ) ),
                _ => CreateTicks( exponent, generation, ( 12, 10 ), ( 60, 5 ) )
            };

            return TickStatus.Normal;
        }

        public override DateTime RoundUp(DateTime toRound, decimal root )
        {
            var monthNum = GetMonthNumber(toRound);

            var modulo = monthNum % root;
            if (modulo == 0)
                return GetFirstDay(toRound, 1);

            return GetFirstDay(toRound, root - modulo + 1);
        }

        public override DateTime RoundDown(DateTime toRound, decimal root )
        {
            var monthNum = GetMonthNumber(toRound);

            var modulo = monthNum % root;
            if (modulo == 0)
                return GetFirstDay(toRound, 0);

            return GetFirstDay(toRound, -modulo);
        }

        public static int GetMonthNumber( DateTime dt ) => dt.Year * 12 + dt.Month;

        public static DateTime GetFirstDay( DateTime dt, decimal delta )
        {
            var deltaYears = (int) delta / 12;
            var deltaMonths = (int) ( delta - 12 * deltaYears );

            var retYear = dt.Year + deltaYears;
            var retMonth = dt.Month + deltaMonths;

            switch( retMonth )
            {
                case 0:
                    retYear--;
                    retMonth = 12;

                    break;

                case < 0:
                    retYear--;
                    retMonth += 13;

                    break;

                case > 12:
                    retYear++;
                    retMonth -= 12;

                    break;
            }

            return new DateTime( retYear, retMonth, 1 );
        }
    }
}