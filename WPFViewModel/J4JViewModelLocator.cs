#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'WPFViewModel' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.ComponentModel;
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
        private readonly ViewModelDependencyBuilder _vmDepBuilder;

        protected J4JViewModelLocator(
            string publisher,
            string appName,
            string? dataProtectionPurpose = null )
            : base( publisher, appName, dataProtectionPurpose )
        {
            _vmDepBuilder = new ViewModelDependencyBuilder( CachedLogger );
        }

        public bool InDesignMode => DesignerProperties
            .GetIsInDesignMode( new DependencyObject() );

        public override string ApplicationConfigurationFolder =>
            InDesignMode ? AppContext.BaseDirectory : Environment.CurrentDirectory;

        protected virtual void RegisterViewModels( ViewModelDependencyBuilder builder )
        {
        }

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            RegisterViewModels( _vmDepBuilder );

            foreach( var vmd in _vmDepBuilder.ViewModelDependencies )
                if( vmd.IsValid )
                {
                    var regBuilder = builder.RegisterType(
                            InDesignMode
                                ? vmd.DesignTimeType ?? vmd.RunTimeType!
                                : vmd.RunTimeType! )
                        .As( vmd.ViewModelInterface! );

                    if( !vmd.MultipleInstances )
                        regBuilder.SingleInstance();
                }
                else
                {
                    CachedLogger.Error( "ViewModel registration is invalid for Type '{0}'", vmd.ViewModelInterface );
                }
        }
    }
}