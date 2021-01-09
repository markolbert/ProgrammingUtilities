using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public class J4JCompositionRoot
    {
        protected J4JCompositionRoot()
        {
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
        }

        protected virtual void SetupServices(HostBuilderContext hbc, IServiceCollection services)
        {
        }
    }
}
