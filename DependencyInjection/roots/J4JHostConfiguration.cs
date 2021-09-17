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
using Autofac;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Configuration.CommandLine.support;
using J4JSoftware.Logging;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public enum BuildStatus
    {
        NotInitialized,
        NotBuilt,
        Aborted,
        Built
    }

    [Flags]
    public enum Requirements
    {
        Publisher = 1 << 0,
        ApplicationName = 1 << 1,
        OperatingSystem = 1 << 2,

        AllMet = 0,
        AllMissing = Publisher | ApplicationName | OperatingSystem
    }

    public class J4JHostConfiguration
    {
        // used to capture log events during the host building process,
        // when the ultimate J4JLogger instance is not yet available
        private readonly J4JCachedLogger _logger = new();

        public J4JHostConfiguration()
        {
            EnvironmentInitializers = new() { SetupAppEnvironmentInternal };
            ConfigurationInitializers = new();
            DependencyInjectionInitializers = new() { SetupDependencyInjectionInternal };
            ServicesInitializers = new() { SetupServicesInternal };
        }

        public Func<bool> InDesignMode { get; set; } = () => false;
        public string Publisher { get; set; }= string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public string DataProtectionPurpose { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        internal List<Assembly> CommandLineAssemblies { get; } = new();
        public Func<Type?, string, int, string, string>? FilePathTrimmer { get; set; }

        internal List<Action<HostBuilderContext, IConfigurationBuilder>> EnvironmentInitializers { get; }
        internal List<Action<IConfigurationBuilder>> ConfigurationInitializers { get; }
        internal List<Action<HostBuilderContext, ContainerBuilder>> DependencyInjectionInitializers { get; }
        internal List<Action<HostBuilderContext, IServiceCollection>> ServicesInitializers { get; }

        internal Action<J4JLoggerConfiguration>? LoggerInitializer { get; set; }
        internal Action<IOptionCollection>? OptionsInitializer { get; set; }

        internal CommandLineSource? CommandLineSource { get; private set; }
        public IConfiguration? ConfigurationDuringBuild { get; private set; }

        public BuildStatus BuildStatus { get; private set; } = BuildStatus.NotInitialized;

        public void OutputBuildLogger( IJ4JLogger logger ) => logger.OutputCache( _logger );

        public Requirements MissingRequirements
        {
            get
            {
                var retVal = Requirements.AllMet;

                if( !OSNames.Supported.Any( x => x.Equals( OperatingSystem, StringComparison.OrdinalIgnoreCase ) ) )
                    retVal |= Requirements.OperatingSystem;

                if (string.IsNullOrEmpty(Publisher))
                    retVal |= Requirements.Publisher;

                if (string.IsNullOrEmpty(ApplicationName))
                    retVal |= Requirements.ApplicationName;

                return retVal;
            }
        }

        internal void SetupCommandLineParsing(IConfigurationBuilder builder)
        {
            var cmdLineFactory = new J4JCommandLineFactory(CommandLineAssemblies.Distinct(), _logger);

            var parser = cmdLineFactory!.GetParser(OperatingSystem);
            if (parser == null)
            {
                BuildStatus = BuildStatus.Aborted;
                _logger.Fatal("Could not create IParser");

                return;
            }

            builder.AddJ4JCommandLine(parser, _logger, out var options, out var cmdLineSrc);

            if (options == null)
            {
                BuildStatus = BuildStatus.Aborted;
                _logger.Fatal("Could not add J4JCommandLine functionality");

                return;
            }

            OptionsInitializer!(options);

            CommandLineSource = cmdLineSrc;

            options.FinishConfiguration();
        }

        internal void SetupAppEnvironmentInternal(HostBuilderContext hbc, IConfigurationBuilder builder)
        {
            ConfigurationDuringBuild = hbc.Configuration;
        }

        private void SetupDependencyInjectionInternal(HostBuilderContext hbc, ContainerBuilder builder)
        {
            builder.RegisterType<J4JProtection>()
                .WithParameter("purpose", DataProtectionPurpose)
                .As<IJ4JProtection>()
                .SingleInstance();

            builder.RegisterType<DataProtection>()
                .As<IDataProtection>()
                .OnActivating(x => x.Instance.Purpose = DataProtectionPurpose)
                .SingleInstance();

            builder.RegisterType<J4JProtection>()
                .As<IJ4JProtection>()
                .SingleInstance();

            builder.Register(c =>
                {
                    var loggerConfig = new J4JLoggerConfiguration(FilePathTrimmer);
                    LoggerInitializer?.Invoke(loggerConfig);

                    return loggerConfig.CreateLogger();
                })
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.Register(c =>
                {
                    // the application configuration folder for XAML projects (e.g., WPF) depends upon
                    // whether or not the app is running in design mode or run-time mode
                    var retVal = new J4JHostInfo(
                        Publisher,
                        ApplicationName,
                        OperatingSystem,
                        Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            Publisher,
                            ApplicationName),
                        InDesignMode,
                        CommandLineSource
                    );

                    return retVal;
                })
                .AsSelf()
                .SingleInstance();

            builder.Register(c =>
                {
                    var provider = c.Resolve<IDataProtectionProvider>();

                    return new J4JProtection(provider, DataProtectionPurpose);
                })
                .As<IJ4JProtection>()
                .SingleInstance();
        }

        internal void SetupServicesInternal(HostBuilderContext hbc, IServiceCollection services)
        {
            services.AddDataProtection();
        }
    }
}