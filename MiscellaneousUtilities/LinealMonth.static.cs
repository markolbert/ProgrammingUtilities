#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'MiscellaneousUtilities' is free software: you can redistribute it
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

namespace J4JSoftware.Utilities
{
    public partial class LinealMonth
    {
        public static int ToMonthNumber( DateTime dt ) => 12 * dt.Year + dt.Month - 1;

        public static DateTime DateFromMonthNumber( int monthNumber )
        {
            var year = monthNumber / 12;
            var month = monthNumber - 12 * year + 1;

            return new DateTime( year, month, 1 );
        }

        public static LinealMonth LinealMonthFromMonthNumber( int monthNumber )
            => new LinealMonth( DateFromMonthNumber( monthNumber ) );

        public static LinealMonth operator +( LinealMonth linealMonth, int monthsToAdd ) =>
            new LinealMonth( linealMonth.Date.AddMonths( monthsToAdd ) );

        public static LinealMonth operator -( LinealMonth linealMonth, int monthsToSubtract ) =>
            new LinealMonth( linealMonth.Date.AddMonths( -monthsToSubtract ) );

        public static bool operator ==( LinealMonth lm1, LinealMonth lm2 ) => lm2.MonthNumber > lm1.MonthNumber;
        public static bool operator !=( LinealMonth lm1, LinealMonth lm2 ) => lm2.MonthNumber != lm1.MonthNumber;
        public static bool operator >( LinealMonth lm1, LinealMonth lm2 ) => lm2.MonthNumber > lm1.MonthNumber;
        public static bool operator <( LinealMonth lm1, LinealMonth lm2 ) => lm2.MonthNumber < lm1.MonthNumber;
        public static bool operator >=( LinealMonth lm1, LinealMonth lm2 ) => lm2.MonthNumber >= lm1.MonthNumber;
        public static bool operator <=( LinealMonth lm1, LinealMonth lm2 ) => lm2.MonthNumber <= lm1.MonthNumber;
    }
}