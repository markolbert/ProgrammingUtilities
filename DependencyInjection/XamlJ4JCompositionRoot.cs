using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public abstract class XamlJ4JCompositionRoot : J4JCompositionRootBase, IJ4JViewModelLocator
    {
        private readonly Func<bool> _inDesignMode;

        protected XamlJ4JCompositionRoot(
            string publisher,
            string appName,
            Func<bool> inDesignMode,
            string? dataProtectionPurpose = null,
            Type? loggingConfigType = null,
            ILoggerConfigurator? loggerConfigurator = null,
            params Assembly[] loggerChannelAssemblies
        )
            : base( publisher, appName, dataProtectionPurpose, loggingConfigType, loggerConfigurator,
                loggerChannelAssemblies )
        {
            _inDesignMode = inDesignMode;
        }

        public bool InDesignMode => _inDesignMode();

        public override string ApplicationConfigurationFolder =>
            InDesignMode ? AppContext.BaseDirectory : Environment.CurrentDirectory;
    }
}
