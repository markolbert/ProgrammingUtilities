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
using System.IO;
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
        private readonly Type? _loggerConfigType;

        protected J4JCompositionRootBase(
            string publisher,
            string appName,
            string? dataProtectionPurpose = null,
            Type? loggerConfigType = null
            )
        {
            ApplicationName = appName;

            UserConfigurationFolder = Path.Combine(
                Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                publisher,
                appName );

            _dataProtectionPurpose = dataProtectionPurpose ?? GetType().Name;

            if( loggerConfigType != null && !typeof(ILoggerConfig).IsAssignableFrom( loggerConfigType ) )
                CachedLogger.Error( "Supplied logger configuration Type '{0}' does not implement {1}", 
                    loggerConfigType,
                    typeof(ILoggerConfig) );
            else _loggerConfigType = loggerConfigType;
        }

        protected IHostBuilder? HostBuilder { get; private set; }

        // always test to ensure configuration is defined because it may not be if you
        // supplied an invalid loggerConfigType to the constructor
        protected virtual void ConfigureLogger( J4JLogger logger, ILoggerConfig? configuration )
        {
        }

        // CachedLogger is used to capture log events during the host building process,
        // when the ultimate J4JLogger instance is not yet available
        protected J4JCachedLogger CachedLogger { get; } = new();

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

            var loggerReg = builder.RegisterType<J4JLogger>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.Register( c => hbc.Configuration )
                .As<IConfiguration>();

            builder.Register( c =>
                {
                    var retVal = new J4JLogger();

                    var loggerConfig = _loggerConfigType != null
                        ? c.Resolve( _loggerConfigType ) as ILoggerConfig
                        : null;

                    ConfigureLogger( retVal, loggerConfig );

                    return retVal;
                } )
                .As<IJ4JLogger>()
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