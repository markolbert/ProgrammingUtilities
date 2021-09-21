using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;
using Serilog.Events;

namespace J4JSoftware.DependencyInjection
{
    [Obsolete("Use J4JHostConfiguration IHostBuilder instead")]
    public abstract class XamlRoot : CompositionRoot
    {
        private readonly Func<bool> _inDesignMode;

        protected XamlRoot(
            string publisher,
            string appName,
            Func<bool> inDesignMode,
            string? dataProtectionPurpose = null,
            string osName = "Windows",
            Func<Type?, string, int, string, string>? filePathTrimmer = null
        )
            : base( publisher, appName, dataProtectionPurpose, osName, filePathTrimmer )
        {
            _inDesignMode = inDesignMode;
        }

        public bool InDesignMode => _inDesignMode();

        public override string ApplicationConfigurationFolder =>
            InDesignMode ? AppContext.BaseDirectory : Environment.CurrentDirectory;
    }
}
