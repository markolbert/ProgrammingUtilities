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
    public abstract class J4JCompositionRootBase<TJ4JLogger>
        where TJ4JLogger : IJ4JLoggerConfiguration, new()
    {
        private readonly string _dataProtectionPurpose;

        private IChannelConfigProvider? _channelProvider;
        private IJ4JLoggerConfiguration? _loggerConfig;

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
        // when the ultimate IJ4JLogger instance is not yet available
        protected J4JCachedLogger CachedLogger { get; } = new J4JCachedLogger();

        public IHost? Host { get; private set; }
        public bool Initialized => Host != null;
        public string ApplicationName { get; }
        public abstract string ApplicationConfigurationFolder { get; }
        public string UserConfigurationFolder { get; }

        protected void ConfigurationBasedLogging( IChannelConfigProvider provider )
            => _channelProvider = provider;

        protected void StaticConfiguredLogging( IJ4JLoggerConfiguration loggerConfig )
            => _loggerConfig = loggerConfig;

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

            // we configure J4JLogger one of two ways depending upon how we were constructed
            // the channel provider way assumes the configuration information is contained 
            // within the IConfiguration system
            if( _channelProvider == null && _loggerConfig == null )
                throw new NullReferenceException(
                    $"J4JLogger can't be configured. Call either {nameof(ConfigurationBasedLogging)}() or {nameof(StaticConfiguredLogging)}()" );

            if( _channelProvider != null )
            {
                _channelProvider.Source = hbc.Configuration;
                builder.RegisterJ4JLogging<J4JLoggerConfiguration>( _channelProvider );
            }
            else builder.RegisterJ4JLogging( _loggerConfig! );
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