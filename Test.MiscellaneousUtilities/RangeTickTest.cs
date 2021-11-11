using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using FluentAssertions;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

#pragma warning disable 8618

namespace Test.MiscellaneousUtilities
{
    public class RangeTickTest
    {
        private readonly IHost _host;

        public RangeTickTest()
        {
            _host = CreateHost();
        }

        private IHost CreateHost()
        {
            var hostConfig = new J4JHostConfiguration()
                             .Publisher( "J4JSoftware" )
                             .ApplicationName( "Tests.MiscellaneousUtilities" )
                             .AddDependencyInjectionInitializers( SetupDependencyInjection );

            var retVal = hostConfig.Build();
            retVal.Should().NotBeNull();

            return retVal!;
        }

        private void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            builder.RegisterType<RangeCalculator>()
                   .As<IRangeCalculator>();

            builder.RegisterType<TickManagers>()
                   .As<ITickManagers>();

            builder.RegisterAssemblyTypes( typeof( ITickManager ).Assembly )
                   .Where( t => !t.IsAbstract
                                && typeof( ITickManager ).IsAssignableFrom( t )
                                && t.GetConstructors().Where( x => !x.GetParameters().Any() ).Any() )
                   .AsImplementedInterfaces();
        }

        private class ValueHolder<T>
        {
            public T Value { get; set; }
        }

        [ Fact ]
        public void TestDoubleList()
        {
            var values = new List<ValueHolder<double>>();

            var calculator = _host.Services.GetRequiredService<IRangeCalculator>();

            Thread.Sleep( 1 );
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

                // needs work

                //var min = values.Min( x => x.Value );
                //var roundedMin = Math.Round( min, -bestFit!.TickInfo.PowerOfTen );
                //Math.Floor( bestFit!.RangeStart ).Should().Be( roundedMin );

                //var max = values.Max( x => x.Value );
                //var roundedMax = Math.Round( max, -bestFit!.TickInfo.PowerOfTen );
                //Math.Ceiling( bestFit!.RangeEnd ).Should().Be( roundedMax );
            }
        }

        [ Fact ]
        public void TestDateTimeList()
        {
            var values = new List<ValueHolder<DateTime>>();

            var calculator = _host.Services.GetRequiredService<IRangeCalculator>();

            Thread.Sleep( 1 );
            var random = new Random();

            for ( var loop = 0; loop < 1000; loop++ )
            {
                for ( var idx = 0; idx < 100; idx++ )
                {
                    values.Add( new ValueHolder<DateTime>
                                {
                                    Value = DateTime.Today.AddDays( 500 - random.Next( 0, 1001 ) )
                                } );
                }

                calculator.Evaluate( values, values.TickExtractor( x => x.Value ) );
                calculator.Alternatives.Should().NotBeNullOrEmpty();

                var bestFit = calculator.Alternatives.BestByInactiveRegions( 100 );
                bestFit.Should().NotBeNull();
            }
        }

        [ Theory ]
        [ InlineData( -76, 1307, -80, 1310 ) ]
        [ InlineData( -0.5, 5, -0.5, 5 ) ]
        [ InlineData( 0, 0, 0, 0 ) ]
        [ InlineData( 5.5, 5.5, 5.5, 5.5 ) ]
        [ InlineData( -5.5, -5.5, -5.5, -5.5 ) ]
        public void TestDouble( double minValue, double maxValue, double rangeStart, double rangeEnd )
        {
            var calculator = _host.Services.GetRequiredService<IRangeCalculator>();

            calculator.Evaluate( minValue, maxValue );

            calculator.Alternatives.Should().NotBeEmpty();

            var bestFit = calculator.Alternatives.BestByInactiveRegions();
            bestFit.Should().NotBeNull();

            bestFit!.RangeStart.Should().Be( rangeStart );
            bestFit!.RangeEnd.Should().Be( rangeEnd );
        }

        [ Theory ]
        [ InlineData( "2/15/2020", "8/17/2021", "2/1/2020", "8/1/2021" ) ]
        [ InlineData( "6/26/2001", "12/31/2021", "6/1/2001", "12/1/2021" ) ]
        [ InlineData( "6/26/2001", "11/30/2021", "6/1/2001", "11/1/2021" ) ]
        [ InlineData( "6/26/2001", "11/30/2001", "6/1/2001", "11/1/2001" ) ]
        [ InlineData( "6/26/2001", "11/30/2002", "6/1/2001", "11/1/2002" ) ]
        [ InlineData( "6/26/2001", "6/26/2001", "6/1/2001", "6/1/2001" ) ]
        public void TestMonth( string minValue, string maxValue, string rangeStart, string rangeEnd )
        {
            var calculator = _host.Services.GetRequiredService<IRangeCalculator>();

            calculator.Evaluate( DateTime.Parse( minValue ), DateTime.Parse( maxValue ) );

            calculator.Alternatives.Should().NotBeEmpty();

            var bestFit = calculator.Alternatives.BestByInactiveRegions();
            bestFit.Should().NotBeNull();

            bestFit!.RangeStart.Should().Be( MonthNumber.GetMonthNumber( rangeStart ) );
            bestFit!.RangeEnd.Should().Be( MonthNumber.GetMonthNumber( rangeEnd ) );
        }
    }
}
