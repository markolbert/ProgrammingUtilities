using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public class XamlJ4JCompositionRoot<TJ4JLogger> : J4JCompositionRootBase<TJ4JLogger>, IJ4JViewModelLocator
        where TJ4JLogger : IJ4JLoggerConfiguration, new()
    {
        private readonly Func<bool> _inDesignMode;
        private readonly ViewModelDependencyBuilder _vmDepBuilder;

        protected XamlJ4JCompositionRoot(
            string publisher,
            string appName,
            Func<bool> inDesignMode,
            string? dataProtectionPurpose = null
        )
            : base(publisher, appName, dataProtectionPurpose)
        {
            _inDesignMode = inDesignMode;

            _vmDepBuilder = new ViewModelDependencyBuilder(CachedLogger);
        }

        public bool InDesignMode => _inDesignMode();

        public override string ApplicationConfigurationFolder =>
            InDesignMode ? AppContext.BaseDirectory : Environment.CurrentDirectory;

        protected virtual void RegisterViewModels(ViewModelDependencyBuilder builder)
        {
        }

        protected override void SetupDependencyInjection(HostBuilderContext hbc, ContainerBuilder builder)
        {
            base.SetupDependencyInjection(hbc, builder);

            RegisterViewModels(_vmDepBuilder);

            foreach (var vmd in _vmDepBuilder.ViewModelDependencies)
            {
                if (vmd.IsValid)
                {
                    var regBuilder = builder.RegisterType(
                            InDesignMode
                                ? vmd.DesignTimeType ?? vmd.RunTimeType!
                                : vmd.RunTimeType!)
                        .As(vmd.ViewModelInterface!);

                    if (!vmd.MultipleInstances)
                        regBuilder.SingleInstance();
                }
                else
                {
                    CachedLogger.Error("ViewModel registration is invalid for Type '{0}'", vmd.ViewModelInterface);
                }
            }
        }
    }
}
