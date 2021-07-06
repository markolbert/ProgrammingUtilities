using System;
using FluentAssertions;
using J4JSoftware.WPFUtilities;
using Xunit;

namespace Test.WPFUtilities
{
    public class RangeTickTest
    {
        [ Theory ]
        [ InlineData( -76, 1307, -80, 1310 ) ]
        [ InlineData( -0.5, 5, -0.5, 5 ) ]
        [ InlineData( 0, 0, 0, 0 ) ]
        [ InlineData( 5.5, 5.5, 5.5, 5.5 ) ]
        [ InlineData( -5.5, -5.5, -5.5, -5.5 ) ]
        public void TestDouble( double minValue, double maxValue, double rangeStart, double rangeEnd )
        {
            var calculator = new RangeCalculator<DoubleTick>( new DoubleTicks(), null );

            calculator.Evaluate(minValue, maxValue);

            calculator.Alternatives.Should().NotBeEmpty();
            calculator.BestFit.Should().NotBeNull();
            calculator.BestFit!.RangeStart.Should().Be(rangeStart);
            calculator.BestFit!.RangeEnd.Should().Be(rangeEnd);
        }

        [Theory]
        [InlineData("2/15/2020", "8/17/2021", "2/1/2020", "8/1/2021")]
        [InlineData("6/26/2001", "12/31/2021", "1/1/2001", "12/1/2021")]
        [InlineData("6/26/2001", "11/30/2021", "1/1/2001", "12/1/2021")]
        [InlineData("6/26/2001", "11/30/2001", "6/1/2001", "11/1/2001")]
        [InlineData("6/26/2001", "11/30/2002", "6/1/2001", "11/1/2002")]
        [InlineData("6/26/2001", "6/26/2001", "6/1/2001", "6/1/2001")]
        public void TestMonth(string minValue, string maxValue, string rangeStart, string rangeEnd)
        {
            var calculator = new RangeCalculator<MonthNumberTick>( new MonthNumberTicks(), null );

            calculator.Evaluate( MonthNumber.GetMonthNumber( minValue ), MonthNumber.GetMonthNumber( maxValue ) );

            calculator.Alternatives.Should().NotBeEmpty();
            calculator.BestFit.Should().NotBeNull();
            calculator.BestFit!.RangeStart.Should().Be( MonthNumber.GetMonthNumber( rangeStart ) );
            calculator.BestFit!.RangeEnd.Should().Be( MonthNumber.GetMonthNumber( rangeEnd ) );
        }
    }
}
