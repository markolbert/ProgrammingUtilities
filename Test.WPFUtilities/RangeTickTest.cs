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
        [ InlineData( 5, 5, 5, 5 ) ]
        [ InlineData( 5.5, 5.5, 5, 6 ) ]
        [ InlineData( -5.5, -5.5, -6, -5 ) ]
        [ InlineData( -965, -7, -970, 0 ) ]
        public void TestDecimal( decimal minValue, decimal maxValue, decimal rangeStart, decimal rangeEnd )
        {
            var calculator = CompositionRoot.Default.GetRangeCalculator<decimal>();

            calculator.GetAlternatives( minValue, maxValue, out var alternates )
                .Should()
                .BeTrue();

            calculator.GetBestFit( minValue, maxValue, out var bestFit )
                .Should()
                .BeTrue();

            bestFit!.RangeStart.Should().Be( rangeStart );
            bestFit.RangeEnd.Should().Be( rangeEnd );

            var roundedUp = calculator.RoundUp( maxValue, bestFit!.MinorTickWidth );
            var roundedDown = calculator.RoundDown( minValue, bestFit.MinorTickWidth );
        }

        [ Theory ]
        [ InlineData( -76, 1307,-80, 1310 ) ]
        [ InlineData( -1, 5, -1, 5 ) ]
        [ InlineData( 0, 0, 0, 0 ) ]
        [ InlineData( 5, 5, 5, 5 ) ]
        public void TestInt( int minValue, int maxValue, int rangeStart, int rangeEnd )
        {
            var calculator = CompositionRoot.Default.GetRangeCalculator<int>();

            calculator.GetAlternatives( minValue, maxValue, out var alternates )
                .Should()
                .BeTrue();

            calculator.GetBestFit( minValue, maxValue, out var bestFit )
                .Should()
                .BeTrue();

            bestFit!.RangeStart.Should().Be(rangeStart);
            bestFit.RangeEnd.Should().Be(rangeEnd);

            var roundedUp = calculator.RoundUp( maxValue, bestFit!.MinorTickWidth );
            var roundedDown = calculator.RoundDown( minValue, bestFit.MinorTickWidth );
        }

        [ Theory ]
        [ InlineData( -76, 1307, -80, 1310 ) ]
        [ InlineData( -0.5, 5, -0.5, 5 ) ]
        [ InlineData( 0, 0, 0, 0 ) ]
        [ InlineData( 5.5, 5.5, 5, 6 ) ]
        [ InlineData( -5.5, -5.5, -6, -5 ) ]
        public void TestDouble( double minValue, double maxValue, double rangeStart, double rangeEnd )
        {
            var calculator = CompositionRoot.Default.GetRangeCalculator<double>();

            calculator.GetAlternatives( minValue, maxValue, out var alternates )
                .Should()
                .BeTrue();

            calculator.GetBestFit( minValue, maxValue, out var bestFit )
                .Should()
                .BeTrue();

            bestFit!.RangeStart.Should().Be(rangeStart);
            bestFit.RangeEnd.Should().Be(rangeEnd);

            var roundedUp = calculator.RoundUp( maxValue, bestFit!.MinorTickWidth );
            var roundedDown = calculator.RoundDown( minValue, bestFit.MinorTickWidth );
        }

        [ Theory ]
        [ InlineData( "2/15/2020", "8/17/2021", "12/1/2019", "10/1/2021" ) ]
        [ InlineData( "6/26/2001", "12/31/2021", "12/1/2000", "1/1/2022" ) ]
        [ InlineData( "6/26/2001", "11/30/2021", "12/1/2000", "1/1/2022" ) ]
        [ InlineData( "6/26/2001", "6/26/2001", "6/1/2001", "7/1/2001" ) ]
        public void TestMonth( string minValue, string maxValue, string rangeStart, string rangeEnd )
        {
            var minDT = DateTime.Parse( minValue );
            var maxDT = DateTime.Parse( maxValue );

            var calculator = CompositionRoot.Default.GetRangeCalculator<DateTime>();

            calculator.GetAlternatives( minDT, maxDT, out var alternates )
                .Should()
                .BeTrue();

            calculator.GetBestFit( minDT, maxDT, out var bestFit )
                .Should()
                .BeTrue();

            bestFit!.RangeStart.Should().Be( DateTime.Parse( rangeStart ) );
            bestFit.RangeEnd.Should().Be( DateTime.Parse( rangeEnd ) );

            var roundedUp = calculator.RoundUp( maxDT, bestFit!.MinorTickWidth );
            var roundedDown = calculator.RoundDown( maxDT, bestFit.MinorTickWidth );
        }
    }
}
