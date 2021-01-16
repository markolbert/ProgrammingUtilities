using System;
using System.Collections.Generic;
using Autofac;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public class J4JCompositionRoot<TJ4JLogger> : J4JCompositionRoot
        where TJ4JLogger : IJ4JLoggerConfiguration, new()
    {
        private readonly Dictionary<Type, string> _channels = new();

        private J4JCachedLogger? _cachedLogger = null;

        protected J4JCompositionRoot( string? dataProtectionPurpose = null )
            : base( dataProtectionPurpose )
        {
        }

        public ChannelInformation ChannelInformation { get; } = new();
        public string LoggingSectionKey { get; set; } = "Logging";
        public bool IncludeLastEvent { get; set; }
        public CachedLoggerScope CachedLoggerScope { get; set; } = CachedLoggerScope.None;

        public J4JCachedLogger? GetCachedLogger()
        {
            switch( CachedLoggerScope )
            {
                case CachedLoggerScope.MultipleInstances:
                    return new J4JCachedLogger();

                case CachedLoggerScope.SingleInstance:
                    _cachedLogger ??= new J4JCachedLogger();
                    return _cachedLogger;
            }

            return null;
        }

        public IJ4JLogger GetJ4JLogger() => Host?.Services.GetRequiredService<IJ4JLogger>()!;

        public void OutputCachedLogger()
        {
            if( CachedLoggerScope == CachedLoggerScope.None )
                return;

            GetJ4JLogger().OutputCache( GetCachedLogger()!.Cache );
        }

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            var factory = new ChannelFactory( 
                hbc.Configuration, 
                ChannelInformation, 
                LoggingSectionKey,
                IncludeLastEvent );

            builder.RegisterJ4JLogging<J4JLoggerConfiguration>(factory);
        }
    }
}