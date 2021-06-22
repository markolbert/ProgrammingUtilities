using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using J4JSoftware.WPFUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Events;

namespace Test.WPFUtilities
{
    public class CompositionRoot : J4JCompositionRoot<J4JLoggerConfiguration>
    {
        static CompositionRoot()
        {
            Default = new CompositionRoot();
            Default.Initialize();
        }

        public static CompositionRoot Default { get; }

        private CompositionRoot()
            : base("J4JSoftware", "Test.WPFUtilities")
        {
            var logConfig = new J4JLoggerConfiguration()
            {
                EventElements = EventElements.All,
                MultiLineEvents = true,
                SourceRootPath = "C:/Programming/RetirementModeling"
            };

            logConfig.Channels.Add( new DebugConfig
            {
                EventElements = EventElements.All, 
                MinimumLevel = LogEventLevel.Verbose
            } );

            StaticConfiguredLogging( logConfig );
        }

        public IRangeCalculators RangeCalculators => Host!.Services.GetRequiredService<IRangeCalculators>();

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            builder.RegisterType<RangeCalculators>()
                .As<IRangeCalculators>();

            builder.RegisterAssemblyTypes( typeof(RangeCalculator<>).Assembly )
                .Where( t => !t.IsAbstract
                             && typeof(IRangeCalculator).IsAssignableFrom( t )
                             && t.GetConstructors().Any() )
                .AsImplementedInterfaces();
        }
    }
}
