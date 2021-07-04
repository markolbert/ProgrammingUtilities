using System;

namespace J4JSoftware.WPFUtilities
{
    public static class MonthNumber
    {
        public static int GetMonthNumber(DateTime dt) => dt.Year * 12 + dt.Month - 1;

        public static int GetMonthNumber(string dateText)
        {
            if (DateTime.TryParse(dateText, out var result))
                return GetMonthNumber(result);

            return GetMonthNumber(DateTime.MinValue);
        }

        public static DateTime DateFromMonthNumber(int monthNumber)
        {
            var year = monthNumber / 12;
            var month = monthNumber - 12 * year + 1;

            return new DateTime(year, month, 1);
        }
    }
}