using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using J4JSoftware.DependencyInjection.Deprecated;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public abstract class XamlJ4JCompositionRoot<TLoggerConfig> : J4JCompositionRootBase, IJ4JViewModelLocator
    {
        private readonly Func<bool> _inDesignMode;
        private readonly ViewModelDependencyBuilder? _vmDepBuilder;

        protected XamlJ4JCompositionRoot(
            string publisher,
            string appName,
            Func<bool> inDesignMode,
            string? dataProtectionPurpose = null,
            Type? loggerConfigType = null,
            bool useViewModelDependency = false
        )
            : base(publisher, appName, dataProtectionPurpose, loggerConfigType)
        {
            _inDesignMode = inDesignMode;

            if( useViewModelDependency )
                _vmDepBuilder = new ViewModelDependencyBuilder(CachedLogger);
        }

        public bool InDesignMode => _inDesignMode();

        public override string ApplicationConfigurationFolder =>
            InDesignMode ? AppContext.BaseDirectory : Environment.CurrentDirectory;

        protected virtual void RegisterViewModels(ViewModelDependencyBuilder builder)
        {
            if( _vmDepBuilder == null )
                throw new NotSupportedException(
                    $"You are trying to register ViewModel dependencies, a deprecated feature, without having enabled them in the constructor call" );
        }

        protected override void SetupDependencyInjection(HostBuilderContext hbc, ContainerBuilder builder)
        {
            base.SetupDependencyInjection(hbc, builder);

            if( _vmDepBuilder == null )
                return;

            RegisterViewModels( _vmDepBuilder );

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
