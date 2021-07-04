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
        [ InlineData( -0.5, 5, -0.5, 5) ]
        [ InlineData( 0, 0, 0, 0) ]
        [ InlineData( 5, 5, 5, 5) ]
        [ InlineData( 5.5, 5.5, 5, 6) ]
        [ InlineData( -5.5, -5.5, -6, -5) ]
        [ InlineData( -965, -7, -970, 0) ]
        public void TestDecimal( decimal minValue, decimal maxValue, decimal rangeStart, decimal rangeEnd )
        {
            var calculator = CompositionRoot.Default.GetRangeCalculator<decimal>();

            calculator.Evaluate( minValue, maxValue );

            calculator.Alternatives.Should().NotBeEmpty();
            calculator.BestFit.Should().NotBeNull();
            calculator.BestFit!.RangeStart.Should().Be( rangeStart );
            calculator.BestFit!.RangeEnd.Should().Be( rangeEnd );
        }

        [ Theory ]
        [ InlineData( -76, 1307,-80, 1310 ) ]
        [ InlineData( -1, 5, -1, 5 ) ]
        [ InlineData( 0, 0, 0, 0 ) ]
        [ InlineData( 5, 5, 5, 5 ) ]
        public void TestInt( int minValue, int maxValue, int rangeStart, int rangeEnd )
        {
            var calculator = CompositionRoot.Default.GetRangeCalculator<int>();

            calculator.Evaluate(minValue, maxValue);

            calculator.Alternatives.Should().NotBeEmpty();
            calculator.BestFit.Should().NotBeNull();
            calculator.BestFit!.RangeStart.Should().Be(rangeStart);
            calculator.BestFit!.RangeEnd.Should().Be(rangeEnd);
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

            calculator.Evaluate(minValue, maxValue);

            calculator.Alternatives.Should().NotBeEmpty();
            calculator.BestFit.Should().NotBeNull();
            calculator.BestFit!.RangeStart.Should().Be(rangeStart);
            calculator.BestFit!.RangeEnd.Should().Be(rangeEnd);
        }

        [ Theory ]
        [ InlineData( "2/15/2020", "8/17/2021", "12/1/2019", "10/1/2021" ) ]
        [ InlineData( "6/26/2001", "12/31/2021", "12/1/2000", "1/1/2022" ) ]
        [ InlineData( "6/26/2001", "11/30/2021", "12/1/2000", "1/1/2022" ) ]
        [ InlineData( "6/26/2001", "6/26/2001", "6/1/2001", "7/1/2001" ) ]
        public void TestMonth( string minValue, string maxValue, string rangeStart, string rangeEnd )
        {
            var calculator = CompositionRoot.Default.GetRangeCalculator<DateTime>();

            calculator.Evaluate(DateTime.Parse(minValue), DateTime.Parse(maxValue));

            calculator.Alternatives.Should().NotBeEmpty();
            calculator.BestFit.Should().NotBeNull();
            calculator.BestFit!.RangeStart.Should().Be( DateTime.Parse( rangeStart ) );
            calculator.BestFit!.RangeEnd.Should().Be( DateTime.Parse( rangeEnd ) );
        }
    }
}
