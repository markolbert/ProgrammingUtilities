using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using J4JSoftware.Utilities;
using J4JSoftware.WPFUtilities;
using Xunit;

namespace Test.WPFUtilities
{
    public class RangeTickTest
    {
        private class ValueHolder<T>
        {
            public T Value { get; set; }
        }

        [ Fact ]
        public void TestDoubleList()
        {
            var values = new List<ValueHolder<double>>();

            var calculator = CompositionRoot.Default.GetRangeCalculator();

            Thread.Sleep(1);
            var random = new Random();

            for ( var loop = 0; loop < 1000; loop++ )
            {
                for( var idx = 0; idx < 100; idx++ )
                {
                    values.Add( new ValueHolder<double> { Value = 1000 * random.NextDouble() } );
                }

                calculator.Evaluate( values, values.TickExtractor( x => x.Value ) );
                calculator.Alternatives.Should().NotBeNullOrEmpty();

                var bestFit = calculator.Alternatives.BestByInactiveRegions( 100 );
                bestFit.Should().NotBeNull();

                var min = values.Min( x => x.Value );
                min = Math.Floor( min );
                Math.Floor( bestFit!.RangeStart ).Should().Be( min );

                var max = values.Max( x => x.Value );
                max = Math.Ceiling( max );
                Math.Ceiling( bestFit!.RangeEnd ).Should().Be( max );
            }
        }

        [Fact]
        public void TestDateTimeList()
        {
            var values = new List<ValueHolder<DateTime>>();

            var calculator = CompositionRoot.Default.GetRangeCalculator();

            Thread.Sleep(1);
            var random = new Random();

            for (var loop = 0; loop < 1000; loop++)
            {
                for (var idx = 0; idx < 100; idx++)
                {
                    values.Add( new ValueHolder<DateTime>
                    {
                        Value = DateTime.Today.AddDays( 500 - random.Next( 0, 1001 ) )
                    } );
                }

                calculator.Evaluate(values, values.TickExtractor(x => x.Value));
                calculator.Alternatives.Should().NotBeNullOrEmpty();

                var bestFit = calculator.Alternatives.BestByInactiveRegions(100);
                bestFit.Should().NotBeNull();
            }
        }

        [Theory ]
        [ InlineData( -76, 1307, -80, 1310 ) ]
        [ InlineData( -0.5, 5, -0.5, 5 ) ]
        [ InlineData( 0, 0, 0, 0 ) ]
        [ InlineData( 5.5, 5.5, 5.5, 5.5 ) ]
        [ InlineData( -5.5, -5.5, -5.5, -5.5 ) ]
        public void TestDouble( double minValue, double maxValue, double rangeStart, double rangeEnd )
        {
            var calculator = CompositionRoot.Default.GetRangeCalculator();

            calculator.Evaluate(minValue, maxValue);

            calculator.Alternatives.Should().NotBeEmpty();

            var bestFit = calculator.Alternatives.BestByInactiveRegions();
            bestFit.Should().NotBeNull();

            bestFit!.RangeStart.Should().Be(rangeStart);
            bestFit!.RangeEnd.Should().Be(rangeEnd);
        }

        [Theory]
        [InlineData("2/15/2020", "8/17/2021", "2/1/2020", "8/1/2021")]
        [InlineData("6/26/2001", "12/31/2021", "6/1/2001", "12/1/2021")]
        [InlineData("6/26/2001", "11/30/2021", "6/1/2001", "11/1/2021")]
        [InlineData("6/26/2001", "11/30/2001", "6/1/2001", "11/1/2001")]
        [InlineData("6/26/2001", "11/30/2002", "6/1/2001", "11/1/2002")]
        [InlineData("6/26/2001", "6/26/2001", "6/1/2001", "6/1/2001")]
        public void TestMonth(string minValue, string maxValue, string rangeStart, string rangeEnd)
        {
            var calculator = CompositionRoot.Default.GetRangeCalculator();

            calculator.Evaluate( DateTime.Parse( minValue), DateTime.Parse(maxValue) );

            calculator.Alternatives.Should().NotBeEmpty();

            var bestFit = calculator.Alternatives.BestByInactiveRegions();
            bestFit.Should().NotBeNull();
            
            bestFit!.RangeStart.Should().Be( MonthNumber.GetMonthNumber( rangeStart ) );
            bestFit!.RangeEnd.Should().Be( MonthNumber.GetMonthNumber( rangeEnd ) );
        }
    }
}
