using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.WPFViewModel
{
    public class J4JViewModelLocator<TJ4JLogger> : J4JCompositionRootBase<TJ4JLogger>, IJ4JViewModelLocator
        where TJ4JLogger : IJ4JLoggerConfiguration, new()
    {
        private readonly List<ViewModelDependency> _vmDependencies = new();

        protected J4JViewModelLocator( 
            string publisher, 
            string appName, 
            string? dataProtectionPurpose = null ) 
            : base( publisher, appName, dataProtectionPurpose )
        {
        }

        public bool InDesignMode => System.ComponentModel.DesignerProperties
            .GetIsInDesignMode( new DependencyObject() );

        public override string ApplicationConfigurationFolder => AppContext.BaseDirectory;

        protected ViewModelDependency RegisterViewModel()
        {
            var retVal = new ViewModelDependency( CachedLogger );
            _vmDependencies.Add( retVal );

            return retVal;
        }

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            foreach( var vmd in _vmDependencies )
            {
                if( vmd.IsValid )
                {
                    var regBuilder = builder.RegisterType( InDesignMode ? vmd.DesignTimeType! : vmd.RunTimeType! )
                        .As( vmd.ViewModelInterface! );

                    if( !vmd.MultipleInstances )
                        regBuilder.SingleInstance();
                }
                else CachedLogger.Error( "ViewModel registration is invalid for Type '{0}'", vmd.ViewModelInterface );
            }
        }
    }
}
