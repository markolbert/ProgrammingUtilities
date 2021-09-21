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
using J4JSoftware.Logging;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public class J4JHostConfiguration
    {
        // used to determine if the host builder is running within a design-time
        // environment (e.g., in a WPF designer)
        private readonly Func<bool> _inDesignMode;

        private string _osName = string.Empty;
        private IOptionCollection? _options;

        public J4JHostConfiguration()
            : this( () => false )
        {
        }

        public J4JHostConfiguration(
            Func<bool> inDesignMode
        )
        {
            _inDesignMode = inDesignMode;

            ConfigurationInitializers.Add( SetupConfiguration );

            DependencyInjectionInitializers.Add( SetupDependencyInjection );
            DependencyInjectionInitializers.Add( SetupLogging );

            ServicesInitializers.Add( SetupServices );

            CommandLineTextToValueConverters.AddRange( BindabilityValidator.GetBuiltInConverters( Logger ) );
        }

        // used to capture log events during the host building process,
        // when the ultimate J4JLogger instance is not yet available
        public J4JCachedLogger Logger { get; } = new();

        internal string Publisher { get; set; }= string.Empty;
        internal string ApplicationName { get; set; } = string.Empty;
        internal string DataProtectionPurpose { get; set; } = string.Empty;

        internal CommandLineOperatingSystems OperatingSystem { get; set; } = CommandLineOperatingSystems.Undefined;
        internal bool CaseSensitiveFileSystem { get; set; } = false;

        internal StringComparison FileSystemTextComparison =>
            CaseSensitiveFileSystem ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        internal List<ConfigurationFile> ApplicationConfigurationFiles { get; } = new();
        internal List<ConfigurationFile> UserConfigurationFiles { get; } = new();

        internal IAvailableTokens? CommandLineAvailableTokens { get; set; }
        internal IBindabilityValidator? CommandLineBindabilityValidator { get; set; }
        internal List<ITextToValue> CommandLineTextToValueConverters { get; } = new();
        internal IOptionsGenerator? CommandLineOptionsGenerator { get; set; }
        internal List<ICleanupTokens> CommandLineCleanupProcessors { get; } = new();
        internal Action<IOptionCollection>? CommandLineOptionsInitializer { get; set; }

        internal CommandLineSource? CommandLineSource { get; private set; }

        internal Action<IConfiguration, J4JLoggerConfiguration>? LoggerInitializer { get; set; }
        internal Func<Type?, string, int, string, string>? FilePathTrimmer { get; set; }
        internal NetEventConfiguration? NetEventConfiguration { get; set; }

        internal List<Action<HostBuilderContext, IConfigurationBuilder>> EnvironmentInitializers { get; } = new();
        internal List<Action<IConfigurationBuilder>> ConfigurationInitializers { get; } = new();
        internal List<Action<HostBuilderContext, ContainerBuilder>> DependencyInjectionInitializers { get; } = new();
        internal List<Action<HostBuilderContext, IServiceCollection>> ServicesInitializers { get; } = new();

        public string ApplicationConfigurationFolder =>
            _inDesignMode() ? AppContext.BaseDirectory : Environment.CurrentDirectory;

        public string UserConfigurationFolder => Path.Combine(
            Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
            Publisher,
            ApplicationName );

        public J4JHostBuildStatus BuildStatus { get; private set; } = J4JHostBuildStatus.NotInitialized;

        public void OutputBuildLogger( IJ4JLogger logger ) => logger.OutputCache( Logger );
        
        public J4JHostRequirements MissingRequirements
        {
            get
            {
                var retVal = J4JHostRequirements.AllMet;

                if (string.IsNullOrEmpty(Publisher))
                    retVal |= J4JHostRequirements.Publisher;

                if (string.IsNullOrEmpty(ApplicationName))
                    retVal |= J4JHostRequirements.ApplicationName;

                // if we're not utilize the command line subsystem there's nothing
                // else to check
                if( CommandLineOptionsInitializer == null )
                    return retVal;

                // if default command line parameters were selected we don't need to
                // check the individual command line requirements because we'll just
                // create defaults as needed
                if( OperatingSystem != CommandLineOperatingSystems.Undefined )
                    return retVal;

                if( CommandLineAvailableTokens == null )
                    retVal |= J4JHostRequirements.AvailableTokens;

                if( CommandLineBindabilityValidator == null )
                    retVal |= J4JHostRequirements.BindabilityValidators;

                if( CommandLineOptionsGenerator == null )
                    retVal |= J4JHostRequirements.OptionsGenerator;

                return retVal;
            }
        }

        private void SetupConfiguration( IConfigurationBuilder builder )
        {
            foreach( var configFile in ApplicationConfigurationFiles
                .Distinct( CaseSensitiveFileSystem? ConfigurationFile.CaseSensitiveComparer : ConfigurationFile.CaseInsensitiveComparer ))
            {
                var filePath = Path.IsPathRooted( configFile.FilePath )
                    ? configFile.FilePath
                    : Path.Combine( ApplicationConfigurationFolder, configFile.FilePath );

                builder.AddJsonFile( filePath, configFile.Optional, configFile.ReloadOnChange );
            }

            foreach (var configFile in UserConfigurationFiles
                .Distinct(CaseSensitiveFileSystem ? ConfigurationFile.CaseSensitiveComparer : ConfigurationFile.CaseInsensitiveComparer))
            {
                var filePath = Path.IsPathRooted(configFile.FilePath)
                    ? configFile.FilePath
                    : Path.Combine(UserConfigurationFolder, configFile.FilePath);

                builder.AddJsonFile(filePath, configFile.Optional, configFile.ReloadOnChange);
            }
        }

        internal void SetupCommandLineParsing(IConfigurationBuilder builder)
        {
            // we create default values for missing required parameters, sometimes based on the 
            // type of operating system specified
            CommandLineBindabilityValidator ??= new BindabilityValidator( CommandLineTextToValueConverters, Logger );

            var optionsCollection = new OptionCollection(
                FileSystemTextComparison,
                CommandLineBindabilityValidator!,
                Logger);

            CommandLineOptionsGenerator ??= new OptionsGenerator( optionsCollection, FileSystemTextComparison, Logger );

            CommandLineAvailableTokens ??= OperatingSystem switch
            {
                CommandLineOperatingSystems.Windows => new WindowsTokens(Logger),
                CommandLineOperatingSystems.Linux => new LinuxTokens(Logger),
                _ => throw new ArgumentException("Operating system is undefined")
            };

            var parsingTable = new ParsingTable( CommandLineOptionsGenerator!, Logger );

            var tokenizer = new Tokenizer( CommandLineAvailableTokens!,
                Logger,
                CommandLineCleanupProcessors.ToArray()
            );

            var parser = new Parser( optionsCollection, parsingTable, tokenizer, Logger );

            builder.AddJ4JCommandLine(parser, Logger, out var options, out var cmdLineSrc);

            if (options == null)
            {
                BuildStatus = J4JHostBuildStatus.Aborted;
                Logger.Fatal("Could not add J4JCommandLine functionality");

                return;
            }

            _options = options;

            CommandLineOptionsInitializer!( _options );

            CommandLineSource = cmdLineSrc;

            options.FinishConfiguration();
        }

        private void SetupDependencyInjection(HostBuilderContext hbc, ContainerBuilder builder)
        {
            builder.RegisterType<DataProtection>()
                .As<IDataProtection>()
                .OnActivating(x => x.Instance.Purpose = string.IsNullOrEmpty( DataProtectionPurpose) ? ApplicationName : DataProtectionPurpose)
                .SingleInstance();

            builder.RegisterType<J4JProtection>()
                .As<IJ4JProtection>()
                .SingleInstance();

            builder.Register(c =>
                {
                    // the application configuration folder for XAML projects (e.g., WPF) depends upon
                    // whether or not the app is running in design mode or run-time mode
                    var retVal = new J4JHostInfo(
                        Publisher,
                        ApplicationName,
                        CommandLineAvailableTokens,
                        _inDesignMode,
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

            // expose IOptionCollection when J4JCommandLine subsystem is being used
            if( _options != null )
                builder.Register( c => _options )
                    .AsImplementedInterfaces()
                    .SingleInstance();
        }

        private void SetupLogging( HostBuilderContext hbc, ContainerBuilder builder )
        {
            builder.Register(c =>
                {
                    var loggerConfig = new J4JLoggerConfiguration(FilePathTrimmer);

                    if( NetEventConfiguration != null )
                    {
                        var outputTemplate = NetEventConfiguration.OutputTemplate ?? "[{Level:u3}] {Message:lj}";

                        loggerConfig.NetEvent( outputTemplate: outputTemplate,
                            restrictedToMinimumLevel: NetEventConfiguration.MinimumLevel );
                    }

                    LoggerInitializer?.Invoke(hbc.Configuration, loggerConfig);

                    return loggerConfig.CreateLogger();
                })
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        private void SetupServices(HostBuilderContext hbc, IServiceCollection services)
        {
            services.AddDataProtection();
        }
    }
}