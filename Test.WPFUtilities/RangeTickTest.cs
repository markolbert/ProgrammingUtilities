using System;
using FluentAssertions;
using J4JSoftware.WPFUtilities;
using Xunit;

namespace Test.WPFUtilities
{
    public class RangeTickTest
    {
        [Theory]
        [InlineData(-76, 1307, 2)]
        [InlineData(-0.5, 5, 2)]
        [InlineData(0, 0, 2)]
        [InlineData(5, 5, 2)]
        public void TestDecimal( decimal minValue, decimal maxValue, int minPowerOfTen )
        {
            var calculators = CompositionRoot.Default.RangeCalculators;

            calculators.CalculateAlternatives( minValue, maxValue, out var alternates, minPowerOfTen )
                .Should()
                .BeTrue();
        }

        [Theory]
        [InlineData(-76, 1307, 2)]
        [InlineData(-1, 5, 2)]
        [InlineData(0, 0, 2)]
        [InlineData(5, 5, 2)]
        public void TestInt(int minValue, int maxValue, int minPowerOfTen)
        {
            var calculators = CompositionRoot.Default.RangeCalculators;

            calculators.CalculateAlternatives( minValue, maxValue, out var alternates, minPowerOfTen )
                .Should()
                .BeTrue();
        }

        [Theory]
        [InlineData(-76, 1307, 2)]
        [InlineData(-0.5, 5, 2)]
        [InlineData(0, 0, 2)]
        [InlineData(5.5, 5.5, 2)]
        public void TestDouble(double minValue, double maxValue, int minPowerOfTen)
        {
            var calculators = CompositionRoot.Default.RangeCalculators;

            calculators.CalculateAlternatives( minValue, maxValue, out var alternates, minPowerOfTen )
                .Should()
                .BeTrue();
        }

        [Theory]
        [InlineData("2/15/2020", "8/17/2021", 2)]
        [InlineData("6/26/2001", "12/31/2021", 2)]
        [InlineData("6/26/2001", "11/30/2021", 2)]
        [InlineData("6/26/2001", "6/26/2001", 2)]
        public void TestMonth(string minValue, string maxValue, int minPowerOfTen)
        {
            var calculators = CompositionRoot.Default.RangeCalculators;

            calculators.CalculateAlternatives(
                    DateTime.Parse( minValue ),
                    DateTime.Parse( maxValue ),
                    out var alternates,
                    minPowerOfTen )
                .Should()
                .BeTrue();
        }
    }
}
