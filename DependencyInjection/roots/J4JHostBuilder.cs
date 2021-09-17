using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Configuration.CommandLine.support;
using J4JSoftware.Logging;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Data;

namespace J4JSoftware.DependencyInjection
{
    public class J4JHostBuilder
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

        private BuildStatus _buildStatus = BuildStatus.NotInitialized;
        private Func<bool>? _inDesignMode;
        private string _publisher = string.Empty;
        private string _appName = string.Empty;
        private string _dataProtectionPurpose;
        private string _osName = string.Empty;
        private List<Assembly> _cmdLineAssemblies = new();
        private Func<Type?, string, int, string, string>? _filePathTrimmer;
        private CommandLineSource? _cmdLineSource;

        private List<Action<HostBuilderContext, IConfigurationBuilder>> _envConfigurators = new();
        private List<Action<IConfigurationBuilder>> _configConfigurators = new();
        private List<Action<HostBuilderContext, ContainerBuilder>> _diConfigurators = new();
        private List<Action<HostBuilderContext, IServiceCollection>> _svcConfigurators = new();

        private Action<J4JLoggerConfiguration>? _loggerConfigurator;
        private Action<IOptionCollection>? _optionsConfigurator;

        public J4JHostBuilder()
        {
            _dataProtectionPurpose = GetType().Name;

            _envConfigurators.Add(SetupAppEnvironmentInternal);
            _diConfigurators.Add( SetupDependencyInjectionInternal );
            _svcConfigurators.Add( SetupServicesInternal );
        }

        // Logger is used to capture log events during the host building process,
        // when the ultimate J4JLogger instance is not yet available
        public J4JCachedLogger Logger { get; } = new();

        private IHostBuilder CreateHostBuilder()
        {
            var retVal = new HostBuilder()
                .UseServiceProviderFactory( new AutofacServiceProviderFactory() );

            foreach( var configurator in _envConfigurators )
            {
                retVal.ConfigureAppConfiguration( configurator );
            }

            foreach( var configurator in _configConfigurators )
            {
                retVal.ConfigureHostConfiguration( configurator );
            }

            foreach( var configurator in _diConfigurators )
            {
                retVal.ConfigureContainer( configurator );
            }

            foreach( var configurator in _svcConfigurators )
            {
                retVal.ConfigureServices( configurator );
            }

            return retVal;
        }

        private bool OkayToSet => _buildStatus == BuildStatus.NotBuilt || _buildStatus == BuildStatus.NotInitialized;

        public IConfiguration? ConfigurationDuringBuild { get; private set; }

        #region Configuration and customization

        public J4JHostBuilder OperatingSystem( string osName )
        {
            if( OkayToSet )
                _osName = osName;

            return this;
        }

        public J4JHostBuilder Publisher(string publisher)
        {
            if (OkayToSet)
                _publisher = publisher;

            return this;
        }

        public J4JHostBuilder ApplicationName( string appName )
        {
            if (OkayToSet)
                _appName = appName;

            return this;
        }

        // used to add assemblies to be searched by the J4JCommandLine subsystem
        public J4JHostBuilder AddCommandLineAssembly( params Assembly[] toAdd )
        {
            if (OkayToSet)
                _cmdLineAssemblies.AddRange( toAdd );

            return this;
        }

        public J4JHostBuilder DataProtectionPurpose( string dataProtectionPurpose )
        {
            if (OkayToSet)
                _dataProtectionPurpose = dataProtectionPurpose;

            return this;
        }

        public J4JHostBuilder SourceFilePathTrimmer( Func<Type?, string, int, string, string> filePathTrimmer )
        {
            if (OkayToSet)
                _filePathTrimmer = filePathTrimmer;
    
            return this;
        }

        public J4JHostBuilder AddEnvironmentInitializers(params Action<HostBuilderContext, IConfigurationBuilder>[] configurators)
        {
            if (OkayToSet && configurators.Length > 0)
                _envConfigurators.AddRange(configurators);

            return this;
        }

        public J4JHostBuilder AddConfigurationInitializers( params Action<IConfigurationBuilder>[] configurators )
        {
            if( OkayToSet && configurators.Length > 0 )
                _configConfigurators.AddRange( configurators );

            return this;
        }

        public J4JHostBuilder AddDependencyInjectionInitializers(params Action<HostBuilderContext, ContainerBuilder>[] configurators)
        {
            if (OkayToSet && configurators.Length > 0)
                _diConfigurators.AddRange(configurators);

            return this;
        }

        public J4JHostBuilder AddServicesInitializers(params Action<HostBuilderContext, IServiceCollection>[] configurators)
        {
            if (OkayToSet && configurators.Length > 0)
                _svcConfigurators.AddRange(configurators);

            return this;
        }

        public J4JHostBuilder SetupLogging( Action<J4JLoggerConfiguration> configurator )
        {
            if (OkayToSet)
                _loggerConfigurator = configurator;

            return this;
        }

        public J4JHostBuilder DefineCommandLineOptions( Action<IOptionCollection> configurator )
        {
            if (OkayToSet)
                _optionsConfigurator = configurator;

            return this;
        }

        #endregion

        public IHost? Build( out BuildStatus result, out Requirements missingRequirements, Func<bool>? inDesignMode = null )
        {
            result = _buildStatus;
            missingRequirements = Requirements.AllMet;

            if( string.IsNullOrEmpty( _osName ) )
                missingRequirements |= Requirements.OperatingSystem;

            if (string.IsNullOrEmpty(_publisher))
                missingRequirements |= Requirements.Publisher;

            if( string.IsNullOrEmpty( _appName ) )
                missingRequirements |= Requirements.ApplicationName;

            if( !OkayToSet || missingRequirements != Requirements.AllMet )
                return null;

            // if we got here we're properly initialized
            _buildStatus = BuildStatus.NotBuilt;

            _inDesignMode = inDesignMode;

            // only add the command line parsing code if we need to
            if( _optionsConfigurator != null )
                _configConfigurators.Add( SetupCommandLineParsing );

            IHost? host = null;

            try
            {
                var builder = CreateHostBuilder();

                host = builder.Build();

                if (_buildStatus == BuildStatus.NotBuilt)
                    _buildStatus = BuildStatus.Built;
                else
                {
                    result = _buildStatus;
                    return null;
                }
            }
            catch (Exception e)
            {
                _buildStatus = BuildStatus.Aborted;
                result = _buildStatus;

                Logger.Fatal<string>("Host build failed, exception was {0}", e.Message);

                return null;
            }

            result = _buildStatus;

            // output anything we've logged to the startup/cached logger 
            // to the real logger
            var logger = host!.Services.GetRequiredService<IJ4JLogger>();
            logger.OutputCache(Logger);

            return host;
        }

        #region Define host-building environment

        private void SetupCommandLineParsing( IConfigurationBuilder builder )
        {
            _cmdLineAssemblies = _cmdLineAssemblies.Distinct().ToList();

            var cmdLineFactory = new J4JCommandLineFactory( _cmdLineAssemblies, Logger );

            var parser = cmdLineFactory!.GetParser( _osName );
            if( parser == null )
            {
                _buildStatus = BuildStatus.Aborted;
                Logger.Fatal( "Could not create IParser" );

                return;
            }

            builder.AddJ4JCommandLine( parser, Logger, out var options, out var cmdLineSrc );

            if( options == null )
            {
                _buildStatus = BuildStatus.Aborted;
                Logger.Fatal( "Could not add J4JCommandLine functionality" );

                return;
            }

            _optionsConfigurator!( options );

            _cmdLineSource = cmdLineSrc;

            options.FinishConfiguration();
        }

        private void SetupAppEnvironmentInternal(HostBuilderContext hbc, IConfigurationBuilder builder)
        {
            if (!OkayToSet)
                return;

            ConfigurationDuringBuild = hbc.Configuration;
        }

        private void SetupDependencyInjectionInternal( HostBuilderContext hbc, ContainerBuilder builder )
        {
            if (!OkayToSet)
                return;

            builder.RegisterType<J4JProtection>()
                .WithParameter( "purpose", _dataProtectionPurpose )
                .As<IJ4JProtection>()
                .SingleInstance();

            builder.RegisterType<DataProtection>()
                .As<IDataProtection>()
                .OnActivating( x => x.Instance.Purpose = _dataProtectionPurpose )
                .SingleInstance();

            builder.RegisterType<J4JProtection>()
                .As<IJ4JProtection>()
                .SingleInstance();

            builder.Register( c =>
                {
                    var loggerConfig = new J4JLoggerConfiguration( _filePathTrimmer );
                    _loggerConfigurator?.Invoke( loggerConfig );

                    return loggerConfig.CreateLogger();
                } )
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.Register( c =>
                {
                    // the application configuration folder for XAML projects (e.g., WPF) depends upon
                    // whether or not the app is running in design mode or run-time mode
                    var retVal = new J4JHostInfo(
                        _publisher,
                        _appName,
                        _osName,
                        Path.Combine(
                            Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                            _publisher,
                            _appName ),
                        _inDesignMode,
                        _cmdLineSource
                    );

                    return retVal;
                } )
                .AsSelf()
                .SingleInstance();

            builder.Register( c =>
                {
                    var provider = c.Resolve<IDataProtectionProvider>();

                    return new J4JProtection( provider, _dataProtectionPurpose );
                } )
                .As<IJ4JProtection>()
                .SingleInstance();
        }

        private void SetupServicesInternal(HostBuilderContext hbc, IServiceCollection services)
        {
            if( !OkayToSet )
                return;

            services.AddDataProtection();
        }

        #endregion
    }
}
