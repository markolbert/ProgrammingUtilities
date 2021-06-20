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
        public void TickTest( double minValue, double maxValue, int minPowerOfTen )
        {
            var mgr = new RangeTickManager( minPowerOfTen );

            var alternates = mgr.Calculate( minValue, maxValue );
            alternates.Should().NotBeEmpty();
        }
    }
}
