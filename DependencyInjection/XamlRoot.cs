using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public abstract class XamlRoot : CompositionRoot
    {
        private readonly Func<bool> _inDesignMode;

        protected XamlRoot(
            string publisher,
            string appName,
            Func<bool> inDesignMode,
            string? dataProtectionPurpose = null
        )
            : base( publisher, appName, dataProtectionPurpose )
        {
            _inDesignMode = inDesignMode;
        }

        public bool InDesignMode => _inDesignMode();

        public override string ApplicationConfigurationFolder =>
            InDesignMode ? AppContext.BaseDirectory : Environment.CurrentDirectory;
    }
}
