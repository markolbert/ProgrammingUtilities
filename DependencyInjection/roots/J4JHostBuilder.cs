﻿using System;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        private Action<IConfigurationBuilder>? _configConfigurator;
        private Action<J4JLoggerConfiguration>? _loggerConfigurator;
        private Action<IOptionCollection>? _optionsConfigurator;
        private List<Assembly> _cmdLineAssemblies = new();
        private Func<Type?, string, int, string, string>? _filePathTrimmer;
        private CommandLineSource? _cmdLineSource;

        public J4JHostBuilder()
        {
            _dataProtectionPurpose = GetType().Name;
        }

        // Logger is used to capture log events during the host building process,
        // when the ultimate J4JLogger instance is not yet available
        public J4JCachedLogger Logger { get; } = new();

        // override to change the way the host building process is initialized
        // doing so should be rare
        protected virtual IHostBuilder CreateHostBuilder()
        {
            var retVal = new HostBuilder()
                .UseServiceProviderFactory( new AutofacServiceProviderFactory() );

            retVal.ConfigureAppConfiguration(SetupAppEnvironment);
            retVal.ConfigureHostConfiguration(SetupConfigurationEnvironment);
            retVal.ConfigureContainer<ContainerBuilder>(SetupDependencyInjection);
            retVal.ConfigureServices(SetupServices);

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

        public J4JHostBuilder SetupConfiguration( Action<IConfigurationBuilder> configurator )
        {
            if( OkayToSet )
                _configConfigurator = configurator;

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

        #region Command line access

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

        protected virtual void SetupAppEnvironment(HostBuilderContext hbc, IConfigurationBuilder builder)
        {
            if (!OkayToSet)
                return;

            ConfigurationDuringBuild = hbc.Configuration;
        }

        protected virtual void SetupConfigurationEnvironment(IConfigurationBuilder builder)
        {
            if (!OkayToSet)
                return;

            _configConfigurator?.Invoke( builder );

            // only need to set up J4JCommandLine subsystem if a configurator was defined
            if( _optionsConfigurator == null )
                return;

            _cmdLineAssemblies = _cmdLineAssemblies.Distinct().ToList();

            var cmdLineFactory = new J4JCommandLineFactory(_cmdLineAssemblies, Logger);

            var parser = cmdLineFactory!.GetParser( _osName );
            if( parser == null )
            {
                _buildStatus = BuildStatus.Aborted;
                Logger.Fatal( "Could not create IParser" );

                return;
            }

            builder.AddJ4JCommandLine(parser, Logger, out var options, out var cmdLineSrc);

            if (options == null)
            {
                _buildStatus = BuildStatus.Aborted;
                Logger.Fatal("Could not add J4JCommandLine functionality");

                return;
            }

            _optionsConfigurator( options );
            
            _cmdLineSource = cmdLineSrc;

            options.FinishConfiguration();
        }

        protected virtual void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
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
        }

        protected virtual void SetupServices(HostBuilderContext hbc, IServiceCollection services)
        {
            if( !OkayToSet )
                return;

            services.AddDataProtection();
            //services.AddSingleton<IJ4JProtection, J4JProtection>();
        }

        #endregion
    }
}
