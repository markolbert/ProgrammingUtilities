using System;

namespace J4JSoftware.Utilities
{
    public static class MonthNumber
    {
        public static int GetMonthNumber( DateTime dt ) => dt.Year * 12 + dt.Month - 1;

        public static int GetMonthNumber( string dateText )
        {
            if ( DateTime.TryParse( dateText, out var result ) )
                return GetMonthNumber( result );

            return GetMonthNumber( DateTime.MinValue );
        }

        public static DateTime DateFromMonthNumber( int monthNumber )
        {
            var year = monthNumber / 12;
            var month = monthNumber - 12 * year + 1;

            return new DateTime( year, month, 1 );
        }

        public static DateTime DateFromMonthNumber( double monthNumber )
        {
            var year = (int) monthNumber / 12;
            var month = (int) monthNumber - 12 * year + 1;
            var day = (int) Math.Round( monthNumber - 12 * year - month + 1 );

            day = day <= 0 ? 1 : day;
            var daysInMonth = DateTime.DaysInMonth( year, month );
            day = day > daysInMonth ? daysInMonth : day;

            return new DateTime( year, month, (int) day );
        }
    }
}
