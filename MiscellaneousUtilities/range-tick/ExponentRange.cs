using System;

namespace J4JSoftware.Utilities
{
    internal class ExponentRange
    {
        public static int GetNormalizedValue(int powerOfTen)
        {
            var retVal = 1;

            for (var curExp = 0; curExp < (powerOfTen < 0 ? -powerOfTen : powerOfTen); curExp++)
            {
                retVal *= 10;
            }

            return retVal;
        }

        public ExponentRange(double minValue, double maxValue)
        {
            if (minValue > maxValue)
            {
                var temp = maxValue;
                maxValue = minValue;
                minValue = temp;
            }

            var range = maxValue - minValue;

            var minExp = minValue == 0 ? 0 : (int)Math.Floor(Math.Log10(Math.Abs(minValue)));
            var rangeExp = range == 0 ? 0 : (int)Math.Floor(Math.Log10(Math.Abs(range)));

            if (rangeExp < minExp)
                minExp = rangeExp;

            if (minExp == rangeExp)
                minExp--;

            MinimumExponent = minExp;
            MaximumExponent = rangeExp;
        }

        public int MinimumExponent { get; }
        public int MaximumExponent { get; }
    }
}