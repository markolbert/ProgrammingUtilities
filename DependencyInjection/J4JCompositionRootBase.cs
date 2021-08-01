#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DependencyInjection' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public abstract class J4JCompositionRootBase
    {
        private readonly string _dataProtectionPurpose;
        private readonly List<Assembly> _loggerChannelAssemblies;
        private readonly Type? _loggingConfigType;

        protected J4JCompositionRootBase(
            string publisher,
            string appName,
            string? dataProtectionPurpose = null,
            Type? loggingConfigType = null,
            ILoggerConfigurator? loggerConfigurator = null,
            params Assembly[] loggerChannelAssemblies
            )
        {
            ApplicationName = appName;

            UserConfigurationFolder = Path.Combine(
                Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                publisher,
                appName );

            _dataProtectionPurpose = dataProtectionPurpose ?? GetType().Name;

            _loggingConfigType = loggingConfigType;

            LoggerConfigurator = loggerConfigurator ?? new LoggerConfigurator( GetLoggerChannel );

            _loggerChannelAssemblies = loggerChannelAssemblies.ToList();

            var baseLoggerAssembly = typeof(IJ4JLogger).Assembly;

            if( !_loggerChannelAssemblies.Contains( baseLoggerAssembly ) )
                _loggerChannelAssemblies.Add( baseLoggerAssembly );
        }

        protected IHostBuilder? HostBuilder { get; private set; }

        // CachedLogger is used to capture log events during the host building process,
        // when the ultimate J4JLogger instance is not yet available
        protected J4JCachedLogger CachedLogger { get; } = new();

        // LoggerConfigurator is used to configure the J4JLogger returned by J4JCompositionRootBase.
        protected ILoggerConfigurator LoggerConfigurator { get; }

        // LoggerConfiguration will hold an instance of whatever Type you've declared to contain
        // logging configuration information...if you've specified the Type in the constructor call
        // and registered a concrete class for it.
        protected object? LoggerConfiguration =>
            _loggingConfigType == null ? null : Host!.Services.GetService(_loggingConfigType);

        // always test to ensure configuration is defined because it may not be if you
        // supplied a loggerConfigType to the constructor which you forgot to register
        protected abstract void ConfigureLogger( J4JLogger logger );

        protected Dictionary<string, Type> RegisteredLoggerChannelTypes { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public IChannel? GetLoggerChannel(J4JLogger logger, Type channelType)
        {
            var channelID = channelType.GetCustomAttribute<ChannelIDAttribute>(false);

            if( !typeof(IChannel).IsAssignableFrom( channelType )
                || channelID == null
                || !RegisteredLoggerChannelTypes.ContainsKey( channelID.Name ) )
                return null;
            
            var retVal = (IChannel?) Host!.Services.GetRequiredService( channelType );
            retVal?.SetAssociatedLogger( logger );

            return retVal;
        }

        public IHost? Host { get; private set; }
        public bool Initialized => Host != null;
        public string ApplicationName { get; }
        public abstract string ApplicationConfigurationFolder { get; }
        public string UserConfigurationFolder { get; }
        public IJ4JProtection Protection => Host?.Services.GetRequiredService<IJ4JProtection>()!;

        public void Initialize()
        {
            HostBuilder = new HostBuilder()
                .UseServiceProviderFactory( new AutofacServiceProviderFactory() );

            InitializeInternal();

            Host = HostBuilder.Build();

            // output anything we've logged to the startup/cached logger 
            // to the real logger
            var logger = Host!.Services.GetRequiredService<IJ4JLogger>();
            logger.OutputCache( CachedLogger );

            HostBuilder = null;
        }

        // override to change the way the host building process is initialized
        protected virtual void InitializeInternal()
        {
            if( HostBuilder == null )
                throw new NullReferenceException(
                    $"{nameof(Initialize)}() must be called before {nameof(InitializeInternal)}()" );

            HostBuilder.ConfigureAppConfiguration( SetupAppEnvironment );
            HostBuilder.ConfigureHostConfiguration( SetupConfigurationEnvironment );
            HostBuilder.ConfigureContainer<ContainerBuilder>( SetupDependencyInjection );
            HostBuilder.ConfigureServices( SetupServices );
        }

        protected virtual void SetupAppEnvironment( HostBuilderContext hbc, IConfigurationBuilder builder )
        {
        }

        protected virtual void SetupConfigurationEnvironment( IConfigurationBuilder builder )
        {
        }

        protected virtual void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            builder.RegisterType<J4JProtection>()
                .WithParameter( "purpose", _dataProtectionPurpose )
                .As<IJ4JProtection>()
                .SingleInstance();

            builder.RegisterType<DataProtection>()
                .As<IDataProtection>()
                .OnActivating( x => x.Instance.Purpose = _dataProtectionPurpose )
                .SingleInstance();

            builder.Register( c => hbc.Configuration )
                .As<IConfiguration>();

            // register J4JLogger, as both itself and as IJ4JLogger,
            // because its configuration depends on being able to resolve
            // more than just the interface
            builder.RegisterType<J4JLogger>()
                .OnActivated( x => ConfigureLogger( x.Instance ) )
                .As<IJ4JLogger>()
                .AsSelf()
                .SingleInstance();

            // Register logging channels. This also updates the protected property RegisteredLoggerChannelTypes
            // so resolution of channel types doesn't require access to the builder context
            builder.RegisterAssemblyTypes( _loggerChannelAssemblies.ToArray() )
                .Where( t => !t.IsAbstract
                             && typeof(IChannel).IsAssignableFrom(t)
                             && t.GetConstructors().Any( c =>
                             {
                                 // constructor must be parameterless
                                 var parameters = c.GetParameters();

                                 if( parameters.Length != 0 )
                                     return false;

                                 var attr = t.GetCustomAttribute<ChannelIDAttribute>( false );

                                 if( attr != null )
                                     RegisteredLoggerChannelTypes.Add( attr.Name, t );
                                 else
                                     CachedLogger.Error<Type, string>(
                                         "Found a J4JLogger Channel type ({0}) which does not have a {1}", 
                                         t,
                                         nameof(ChannelIDAttribute) );

                                 return true;
                             } ) )
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();
        }

        protected virtual void SetupServices( HostBuilderContext hbc, IServiceCollection services )
        {
            services.AddDataProtection();
        }

        #region Encryption/decryption

        public bool Protect( string plainText, out string? encrypted )
        {
            encrypted = null;

            var dataProtection = Host?.Services.GetService<IDataProtection>();
            if( dataProtection == null )
                return false;

            var utf8 = new UTF8Encoding();
            var bytesToEncrypt = utf8.GetBytes( plainText );

            try
            {
                var encryptedBytes = dataProtection.Protector.Protect( bytesToEncrypt );
                encrypted = Convert.ToBase64String( encryptedBytes );
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool Unprotect( string encryptedText, out string? decrypted )
        {
            decrypted = null;

            var dataProtection = Host?.Services.GetService<IDataProtection>();
            if( dataProtection == null )
                return false;

            byte[] decryptedBytes;

            try
            {
                var encryptedBytes = Convert.FromBase64String( encryptedText );
                decryptedBytes = dataProtection.Protector.Unprotect( encryptedBytes );
            }
            catch
            {
                return false;
            }

            var utf8 = new UTF8Encoding();
            decrypted = utf8.GetString( decryptedBytes );

            return true;
        }

        #endregion
    }
}