using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public class J4JCompositionRoot
    {
        private readonly string _dataProtectionPurpose;

        protected J4JCompositionRoot( string? dataProtectionPurpose = null )
        {
            _dataProtectionPurpose = dataProtectionPurpose ?? GetType().Name;
        }

        protected IHostBuilder? HostBuilder { get; private set; }

        public IHost? Host { get; private set; }

        public bool Initialized => Host != null;
        public bool UseConsoleLifetime { get; set; }

        public void Initialize()
        {
            HostBuilder = new HostBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory());

            InitializeInternal();

            Host = HostBuilder.Build();

            HostBuilder = null;
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
#pragma warning disable 168
            catch( Exception e )
#pragma warning restore 168
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
#pragma warning disable 168
            catch( Exception e )
#pragma warning restore 168
            {
                return false;
            }

            var utf8 = new UTF8Encoding();
            decrypted = utf8.GetString( decryptedBytes );

            return true;
        }
        
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
        }

        protected virtual void SetupServices(HostBuilderContext hbc, IServiceCollection services)
        {
            services.AddDataProtection();
        }
    }
}
