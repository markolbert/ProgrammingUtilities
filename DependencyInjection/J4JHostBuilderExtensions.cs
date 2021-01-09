using Autofac;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public static class J4JHostBuilderExtensions
    {
        public static HostBuilder AddJ4JLogging( this HostBuilder hostBuilder, IJ4JLoggerConfiguration config )
        {
            hostBuilder.ConfigureContainer<ContainerBuilder>( ( context, builder ) =>
            {
                builder.RegisterJ4JLogging( config );
            } );

            return hostBuilder;
        }

        public static HostBuilder AddJ4JLogging<TJ4JLogger>(this HostBuilder hostBuilder, IChannelFactory channelFactory)
            where TJ4JLogger : IJ4JLoggerConfiguration, new()
        {
            hostBuilder.ConfigureContainer<ContainerBuilder>((context, builder) =>
            {
                builder.RegisterJ4JLogging<TJ4JLogger>( channelFactory );
            });

            return hostBuilder;
        }
    }
}