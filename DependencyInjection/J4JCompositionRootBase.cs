﻿#region license

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

        protected J4JCompositionRootBase(
            string publisher,
            string appName,
            string? dataProtectionPurpose = null )
        {
            ApplicationName = appName;

            UserConfigurationFolder = Path.Combine(
                Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                publisher,
                appName );

            _dataProtectionPurpose = dataProtectionPurpose ?? GetType().Name;
        }

        protected IHostBuilder? HostBuilder { get; private set; }

        // CachedLogger is used to capture log events during the host building process,
        // when the ultimate J4JLogger instance is not yet available
        protected J4JCachedLogger CachedLogger { get; } = new();

        public bool SingleLoggingInstance { get; set; }
        public IHost? Host { get; private set; }
        public bool Initialized => Host != null;
        public string ApplicationName { get; }
        public abstract string ApplicationConfigurationFolder { get; }
        public string UserConfigurationFolder { get; }
        public IJ4JProtection Protection => Host?.Services.GetRequiredService<IJ4JProtection>()!;

        public J4JLogger GetJ4JLogger( Action<J4JLogger>? configureLogger = null )
        {
            configureLogger ??= ConfigureLoggerDefaults;

            var retVal = Host?.Services.GetRequiredService<J4JLogger>()!;
            configureLogger( retVal );

            return retVal;
        }

        protected abstract void ConfigureLoggerDefaults( J4JLogger logger );

        public void Initialize()
        {
            HostBuilder = new HostBuilder()
                .UseServiceProviderFactory( new AutofacServiceProviderFactory() );

            InitializeInternal();

            Host = HostBuilder.Build();

            GetJ4JLogger().OutputCache( CachedLogger );

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
                .AsSelf();

            if( SingleLoggingInstance )
                loggerReg.SingleInstance();
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