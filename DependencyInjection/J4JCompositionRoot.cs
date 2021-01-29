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
    public class J4JCompositionRoot<TJ4JLogger> : J4JCompositionRootBase<TJ4JLogger>, IJ4JCompositionRoot 
        where TJ4JLogger : IJ4JLoggerConfiguration, new()
    {

        protected J4JCompositionRoot(
            string publisher, 
            string appName,
            string? dataProtectionPurpose = null )
        :base(publisher, appName, dataProtectionPurpose)
        {
        }

        public bool UseConsoleLifetime { get; set; }
        public override string ApplicationConfigurationFolder => Environment.CurrentDirectory;

        protected override void InitializeInternal()
        {
            base.InitializeInternal();

            if( UseConsoleLifetime )
                HostBuilder.UseConsoleLifetime();
        }
    }
}