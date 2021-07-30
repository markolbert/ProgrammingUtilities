using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Test.MiscellaneousUtilities
{
    public class CompositionRoot : J4JCompositionRoot
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
        }

        protected override void ConfigureLogger( J4JLogger logger, ILoggerConfig? configuration )
        {
            logger.AddDebug();
        }

        public IRangeCalculator GetRangeCalculator() => Host!.Services.GetRequiredService<IRangeCalculator>();

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            builder.RegisterModule<AutofacRangeTick>();
        }
    }
}
