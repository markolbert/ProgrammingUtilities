#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DependencyInjection' is free software: you can redistribute it
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public abstract class CompositionRoot<TLoggerConfig, TLoggerConfigurator> : CompositionRoot
        where TLoggerConfig: class
        where TLoggerConfigurator: class, ILoggerConfigurator
    {
        private readonly IEnumerable<Assembly>? _loggerChannelAssemblies;

        private TLoggerConfig? _loggerConfig;
        private TLoggerConfigurator? _loggerConfigurator;

        protected CompositionRoot(
            string publisher,
            string appName,
            string? dataProtectionPurpose = null,
            IEnumerable<Assembly>? loggerChannelAssemblies = null
        )
            : base( publisher, appName, dataProtectionPurpose )
        {
            _loggerChannelAssemblies = loggerChannelAssemblies;
        }

        protected override IEnumerable<Assembly> LoggerChannelAssemblies
        {
            get
            {
                foreach( var assembly in base.LoggerChannelAssemblies )
                {
                    yield return assembly;
                }

                if( _loggerChannelAssemblies == null )
                    yield break;

                foreach( var assembly in _loggerChannelAssemblies )
                {
                    yield return assembly;
                }
            }
        }

        protected abstract void RegisterLoggerConfiguration( ContainerBuilder builder );

        protected TLoggerConfig? LoggerConfig
        {
            get
            {
                _loggerConfig ??= Host!.Services.GetRequiredService<TLoggerConfig>();
                return _loggerConfig;
            }
        }

        protected TLoggerConfigurator LoggerConfigurator
        {
            get
            {
                _loggerConfigurator ??= Host!.Services.GetRequiredService<TLoggerConfigurator>();
                return _loggerConfigurator;
            }
        }

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            // Logger configurators require a single constructor parameter, a reference
            // to an object which can provide logging channels...which CompositionRoot<>
            // can do, so we just point the constructor at this instance.
            builder.RegisterType<TLoggerConfigurator>()
                .WithParameter( new PositionalParameter( 0, this ) )
                .AsSelf();

            RegisterLoggerConfiguration( builder );
        }
    }
}