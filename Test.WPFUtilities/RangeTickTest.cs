using System;
using FluentAssertions;
using J4JSoftware.WPFUtilities;
using Xunit;

namespace Test.WPFUtilities
{
    public class RangeTickTest
    {
        [Theory]
        [InlineData(-76, 1307)]
        [InlineData(-0.5, 5)]
        [InlineData(0, 0)]
        [InlineData(5, 5)]
        [InlineData(5.5, 5.5)]
        [InlineData(-5.5, -5.5)]
        public void TestDecimal( decimal minValue, decimal maxValue )
        {
            var calculators = CompositionRoot.Default.RangeCalculators;

            calculators.CalculateAlternatives( minValue, maxValue, out var alternates )
                .Should()
                .BeTrue();

            calculators.GetBestFit( minValue, maxValue, out var bestFit )
                .Should()
                .BeTrue();

            var rounder = new DecimalRangeRounder( bestFit! );

            var roundedUp = rounder.RoundUp( maxValue );
            var roundedDown = rounder.RoundDown( minValue );
        }

        [Theory]
        [InlineData(-76, 1307)]
        [InlineData(-1, 5)]
        [InlineData(0, 0)]
        [InlineData(5, 5)]
        public void TestInt(int minValue, int maxValue)
        {
            var calculators = CompositionRoot.Default.RangeCalculators;

            calculators.CalculateAlternatives( minValue, maxValue, out var alternates )
                .Should()
                .BeTrue();

            calculators.GetBestFit(minValue, maxValue, out var bestFit)
                .Should()
                .BeTrue();

            var rounder = new IntegerRangeRounder(bestFit!);

            var roundedUp = rounder.RoundUp(maxValue);
            var roundedDown = rounder.RoundDown(minValue);
        }

        [Theory]
        [InlineData(-76, 1307)]
        [InlineData(-0.5, 5)]
        [InlineData(0, 0)]
        [InlineData(5.5, 5.5)]
        [InlineData(-5.5, -5.5)]
        public void TestDouble(double minValue, double maxValue)
        {
            var calculators = CompositionRoot.Default.RangeCalculators;

            calculators.CalculateAlternatives( minValue, maxValue, out var alternates )
                .Should()
                .BeTrue();

            calculators.GetBestFit(minValue, maxValue, out var bestFit)
                .Should()
                .BeTrue();

            var rounder = new DoubleRangeRounder(bestFit!);

            var roundedUp = rounder.RoundUp(maxValue);
            var roundedDown = rounder.RoundDown(minValue);
        }

        [Theory]
        [InlineData("2/15/2020", "8/17/2021")]
        [InlineData("6/26/2001", "12/31/2021")]
        [InlineData("6/26/2001", "11/30/2021")]
        [InlineData("6/26/2001", "6/26/2001")]
        public void TestMonth(string minValue, string maxValue)
        {
            var calculators = CompositionRoot.Default.RangeCalculators;

            var minDT = DateTime.Parse( minValue );
            var maxDT = DateTime.Parse( maxValue );

            calculators.CalculateAlternatives( minDT, maxDT, out var alternates )
                .Should()
                .BeTrue();

            calculators.GetBestFit( minDT, maxDT, out var bestFit)
                .Should()
                .BeTrue();

            var rounder = new MonthRangeRounder(bestFit!);

            var roundedUp = rounder.RoundUp(maxDT);
            var roundedDown = rounder.RoundDown(minDT);
        }
    }
}
