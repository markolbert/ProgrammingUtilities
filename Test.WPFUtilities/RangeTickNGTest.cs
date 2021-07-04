using System;
using FluentAssertions;
using J4JSoftware.WPFUtilities;
using Xunit;

namespace Test.WPFUtilities
{
    public class RangeTickNGTest
    {
        [ Theory ]
        [ InlineData( -76, 1307, -80, 1310 ) ]
        [ InlineData( -0.5, 5, -0.5, 5 ) ]
        [ InlineData( 0, 0, 0, 0 ) ]
        [ InlineData( 5.5, 5.5, 5.4, 5.5 ) ]
        [ InlineData( -5.5, -5.5, -5.5, -5.4 ) ]
        public void TestDouble( double minValue, double maxValue, double rangeStart, double rangeEnd )
        {
            var calculator = new RangeCalculatorNG( new DecimalMinorTickEnumerator(), null );

            calculator.Evaluate(minValue, maxValue);

            calculator.Alternatives.Should().NotBeEmpty();
            calculator.BestFit.Should().NotBeNull();
            calculator.BestFit!.RangeStart.Should().Be(rangeStart);
            calculator.BestFit!.RangeEnd.Should().Be(rangeEnd);
        }

        //[ Theory ]
        //[ InlineData( "2/15/2020", "8/17/2021", "12/1/2019", "10/1/2021" ) ]
        //[ InlineData( "6/26/2001", "12/31/2021", "12/1/2000", "1/1/2022" ) ]
        //[ InlineData( "6/26/2001", "11/30/2021", "12/1/2000", "1/1/2022" ) ]
        //[ InlineData( "6/26/2001", "6/26/2001", "6/1/2001", "7/1/2001" ) ]
        //public void TestMonth( string minValue, string maxValue, string rangeStart, string rangeEnd )
        //{
        //    var calculator = CompositionRoot.Default.GetRangeCalculator<DateTime>();

        //    calculator.Evaluate(DateTime.Parse(minValue), DateTime.Parse(maxValue));

        //    calculator.Alternatives.Should().NotBeEmpty();
        //    calculator.BestFit.Should().NotBeNull();
        //    calculator.BestFit!.RangeStart.Should().Be( DateTime.Parse( rangeStart ) );
        //    calculator.BestFit!.RangeEnd.Should().Be( DateTime.Parse( rangeEnd ) );
        //}
    }
}
