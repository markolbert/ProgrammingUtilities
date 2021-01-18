﻿using System;
using System.Collections.Generic;
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
    public class J4JCompositionRoot<TJ4JLogger>
        where TJ4JLogger : IJ4JLoggerConfiguration, new()
    {
        private readonly Dictionary<Type, string> _channels = new();
        private readonly string _dataProtectionPurpose;

        protected J4JCompositionRoot(string publisher, string appName,  string? dataProtectionPurpose = null )
        {
            ApplicationName = appName;
            ApplicationConfigurationFolder = Environment.CurrentDirectory;

            UserConfigurationFolder = Path.Combine(
                Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                publisher,
                appName );
            
            _dataProtectionPurpose = dataProtectionPurpose ?? GetType().Name;
        }

        protected IHostBuilder? HostBuilder { get; private set; }

        // CachedLogger is used to capture log events during the host building process,
        // when the ultimate IJ4JLogger instance is not yet available
        protected J4JCachedLogger CachedLogger { get; } = new J4JCachedLogger();

        public IHost? Host { get; private set; }

        public bool Initialized => Host != null;
        public bool UseConsoleLifetime { get; set; }

        public string ApplicationName { get; }
        public string ApplicationConfigurationFolder { get; }
        public string UserConfigurationFolder { get; }

        public ChannelInformation ChannelInformation { get; } = new();
        protected string LoggingSectionKey { get; set; } = "Logging";
        protected bool IncludeLastEvent { get; set; }
        public IJ4JLogger GetJ4JLogger() => Host?.Services.GetRequiredService<IJ4JLogger>()!;

        public void Initialize()
        {
            HostBuilder = new HostBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory());

            InitializeInternal();

            Host = HostBuilder.Build();

            GetJ4JLogger().OutputCache( CachedLogger.Cache );

            HostBuilder = null;
        }

        // override to change the way the host building process is initialized
        protected virtual void InitializeInternal()
        {
            if( HostBuilder == null )
                throw new NullReferenceException(
                    $"{nameof(Initialize)}() must be called before {nameof(InitializeInternal)}()" );

            if (UseConsoleLifetime)
                HostBuilder.UseConsoleLifetime();

            HostBuilder.ConfigureAppConfiguration(SetupAppEnvironment);
            HostBuilder.ConfigureHostConfiguration(SetupConfigurationEnvironment);
            HostBuilder.ConfigureContainer<ContainerBuilder>(SetupDependencyInjection);
            HostBuilder.ConfigureServices(SetupServices);
        }
        
        protected virtual void SetupAppEnvironment(HostBuilderContext hbc, IConfigurationBuilder builder)
        {
        }

        protected virtual void SetupConfigurationEnvironment(IConfigurationBuilder builder)
        {
        }

        protected virtual void SetupDependencyInjection(HostBuilderContext hbc, ContainerBuilder builder)
        {
            builder.RegisterType<DataProtection>()
                .As<IDataProtection>()
                .OnActivating( x => x.Instance.Purpose = _dataProtectionPurpose )
                .SingleInstance();

            var factory = new ChannelFactory( 
                hbc.Configuration, 
                ChannelInformation, 
                LoggingSectionKey,
                IncludeLastEvent );

            builder.RegisterJ4JLogging<J4JLoggerConfiguration>(factory);
        }

        protected virtual void SetupServices(HostBuilderContext hbc, IServiceCollection services)
        {
            services.AddDataProtection();
        }

        public bool Protect( string plainText, out string? encrypted )
        {
            encrypted = null;

            var dataProtection = Host?.Services.GetService<IDataProtection>();
            if( dataProtection == null )
                return false;

            var utf8 = new UTF8Encoding();
            var bytesToEncrypt = utf8.GetBytes(plainText);

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
            if (dataProtection == null)
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
    }
}