using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Test.MiscellaneousUtilities
{
    public class CompositionRoot : ConsoleRoot
    {
        private static CompositionRoot? _compRoot;

        public static CompositionRoot Default
        {
            get
            {
                if( _compRoot == null )
                {
                    _compRoot = new CompositionRoot();
                    _compRoot.Build();
                }

                return _compRoot!;
            }
        }

        private CompositionRoot()
            : base("J4JSoftware", "Test.WPFUtilities")
        {
        }

        protected override void ConfigureLogger( J4JLoggerConfiguration loggerConfig )
        {
            loggerConfig
                .SerilogConfiguration
                .WriteTo.Debug( outputTemplate: loggerConfig.GetOutputTemplate( true ) );
        }

        public IRangeCalculator GetRangeCalculator() => Host!.Services.GetRequiredService<IRangeCalculator>();

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            builder.RegisterModule<AutofacRangeTick>();
        }
    }
}
