// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of DependencyInjection.
//
// DependencyInjection is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// DependencyInjection is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with DependencyInjection. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Logging;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Serilog.ILogger;

namespace J4JSoftware.DependencyInjection;

public class J4JHostConfiguration
{
    private OptionCollection? _options;
    private StringComparison? _cmdLineTextComparison;
    private string _publisher = string.Empty;
    private string _appName = string.Empty;

    public J4JHostConfiguration(
        AppEnvironment appEnvironment,
        bool registerJ4JHost = true
    )
    {
        BuildLogger = new LoggerConfiguration()
            .WriteTo.InMemory(out var temp)
            .CreateLogger();

        BuildLoggerFactory = new LoggerFactory()
           .AddSerilog( BuildLogger );

        BuildLoggerSink = temp;

        RegisterJ4JHost = registerJ4JHost;
        AppEnvironment = appEnvironment;

        AutoDetectCaseSensitivity();

        ApplicationConfigurationFolder = AppEnvironment == AppEnvironment.WpfDesignMode
            ? AppContext.BaseDirectory
            : Environment.CurrentDirectory;

        ConfigurationInitializers.Add( CoreApplicationJsonConfiguration );
        ConfigurationInitializers.Add( CoreUserJsonConfiguration );

        DependencyInjectionInitializers.Add( CoreDataProtectionDependencyInjection );
        DependencyInjectionInitializers.Add( CoreCommandLineDependencyInjection );
        DependencyInjectionInitializers.Add( CoreHostDependencyInjection );
        DependencyInjectionInitializers.Add( CoreLoggingDependencyInjection );

        ServicesInitializers.Add( CoreDataProtectionServices );
    }

    // used to capture log events during the host building process,
    // when the ultimate ILogger instance is not yet available
    public ILogger BuildLogger { get; }
    public ILoggerFactory BuildLoggerFactory { get; }
    public InMemorySink BuildLoggerSink { get; }

    internal string Publisher
    {
        get => _publisher;
        set => _publisher = value;
    }

    internal string ApplicationName
    {
        get => _appName;
        set => _appName = value;
    }

    internal string DataProtectionPurpose { get; set; } = string.Empty;
    internal AppEnvironment AppEnvironment { get; }

    // controls whether the IJ4JHost instance that gets built is itself
    // registered via dependency injection (default = true)
    internal bool RegisterJ4JHost { get; }
    internal IJ4JHost? Host { get; set; }

    internal bool FileSystemCaseSensitivity { get; set; }

    private void AutoDetectCaseSensitivity()
    {
        FileSystemCaseSensitivity = Environment.OSVersion.Platform switch
        {
            PlatformID.MacOSX       => true,
            PlatformID.Unix         => true,
            PlatformID.Win32NT      => false,
            PlatformID.Win32S       => false,
            PlatformID.Win32Windows => false,
            PlatformID.WinCE        => false,
            PlatformID.Xbox         => false,
            _                       => DefaultSensitivity()
        };

        bool DefaultSensitivity()
        {
            BuildLogger.Warning( "Unsupported operating system, case sensitivity set to false" );
            return false;
        }
    }

    internal StringComparison CommandLineTextComparison
    {
        get
        {
            if( _cmdLineTextComparison.HasValue )
                return _cmdLineTextComparison.Value;

            return FileSystemCaseSensitivity ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        }

        set => _cmdLineTextComparison = value;
    }

    internal void ResetCommandLineTextComparison() => _cmdLineTextComparison = null;

    internal List<ConfigurationFile> ApplicationConfigurationFiles { get; } = new();
    internal List<ConfigurationFile> UserConfigurationFiles { get; } = new();

    internal J4JCommandLineConfiguration? CommandLineConfiguration { get; set; }

    internal CommandLineSource? CommandLineSource { get; private set; }

    internal Func<IConfiguration, J4JHostConfiguration, ILogger>? RuntimeLoggerInitializer { get; set; }

    internal List<Action<HostBuilderContext, IConfigurationBuilder>> EnvironmentInitializers { get; } = new();
    internal List<Action<IConfigurationBuilder>> ConfigurationInitializers { get; } = new();
    internal List<Action<HostBuilderContext, ContainerBuilder>> DependencyInjectionInitializers { get; } = new();
    internal List<Action<HostBuilderContext, IServiceCollection>> ServicesInitializers { get; } = new();

    public string ApplicationConfigurationFolder { get; protected set; }

    public virtual bool TryGetUserConfigurationFolder(out string? result)
    {
        result = null;

        if (string.IsNullOrEmpty(_publisher))
            BuildLogger.Error("UserConfigurationFolder is undefined because Publisher is undefined");

        if (string.IsNullOrEmpty(_appName))
            BuildLogger.Error("UserConfigurationFolder is undefined because ApplicationName is undefined");

        if (!string.IsNullOrEmpty(_publisher) && !string.IsNullOrEmpty(_appName))
            result = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Publisher,
                ApplicationName);

        return result != null;
    }

    public J4JHostRequirements MissingRequirements
    {
        get
        {
            var retVal = J4JHostRequirements.AllMet;

            if ( string.IsNullOrEmpty( Publisher ) )
                retVal |= J4JHostRequirements.Publisher;

            if ( string.IsNullOrEmpty( ApplicationName ) )
                retVal |= J4JHostRequirements.ApplicationName;

            // if we're not utilize the command line subsystem there's nothing
            // else to check
            if( CommandLineConfiguration == null )
                return retVal;

            // if default command line parameters were selected we don't need to
            // check the individual command line requirements because we'll just
            // create defaults as needed
            if( CommandLineConfiguration.OperatingSystem != CommandLineOperatingSystems.Customized )
                return retVal;

            if( CommandLineConfiguration.LexicalElements == null )
                retVal |= J4JHostRequirements.AvailableTokens;

            if( CommandLineConfiguration.OptionsGenerator == null )
                retVal |= J4JHostRequirements.OptionsGenerator;

            return retVal;
        }
    }

    private void CoreApplicationJsonConfiguration( IConfigurationBuilder builder )
    {
        foreach( var configFile in ApplicationConfigurationFiles
                   .Distinct( FileSystemCaseSensitivity
                                  ? ConfigurationFile.CaseSensitiveComparer
                                  : ConfigurationFile.CaseInsensitiveComparer ) )
        {
            builder.AddJsonFile( configFile.FilePath, configFile.Optional, configFile.ReloadOnChange );
        }
    }

    private void CoreUserJsonConfiguration(IConfigurationBuilder builder)
    {
        foreach (var configFile in UserConfigurationFiles
                    .Distinct(FileSystemCaseSensitivity
                                  ? ConfigurationFile.CaseSensitiveComparer
                                  : ConfigurationFile.CaseInsensitiveComparer))
        {
            builder.AddJsonFile(configFile.FilePath, configFile.Optional, configFile.ReloadOnChange);
        }
    }

    internal void SetupCommandLineParsing( IConfigurationBuilder builder )
    {
        // we create default values for missing required parameters, sometimes based on the 
        // type of operating system specified
        CommandLineConfiguration!.TextConverters ??= new TextConverters();

        _options = new OptionCollection( CommandLineTextComparison,
                                         CommandLineConfiguration.TextConverters,
                                         BuildLoggerFactory );

        CommandLineConfiguration.OptionsGenerator ??=
            new OptionsGenerator( _options, CommandLineTextComparison, BuildLoggerFactory );

        CommandLineConfiguration.LexicalElements ??= CommandLineConfiguration.OperatingSystem switch
        {
            CommandLineOperatingSystems.Windows =>
                new WindowsLexicalElements( BuildLoggerFactory ),
            CommandLineOperatingSystems.Linux =>
                new LinuxLexicalElements( BuildLoggerFactory ),
            _ => throw new
                J4JDependencyInjectionException( "Operating system is undefined", this )
        };

        var parsingTable = new ParsingTable( CommandLineConfiguration.OptionsGenerator! );

        var tokenizer = new Tokenizer( CommandLineConfiguration.LexicalElements!,
                                       BuildLoggerFactory,
                                       CommandLineConfiguration.CleanupProcessors.ToArray() );

        var parser = new Parser( _options, parsingTable, tokenizer, BuildLoggerFactory );

        builder.AddJ4JCommandLine( parser, out var cmdLineSrc );

        CommandLineConfiguration.OptionsInitializer!( _options );

        CommandLineSource = cmdLineSrc;

        _options.FinishConfiguration();
    }

    private void CoreDataProtectionDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
    {
        builder.RegisterType<DataProtection>()
               .As<IDataProtection>()
               .OnActivating( x => x.Instance.Purpose =
                                  string.IsNullOrEmpty( DataProtectionPurpose )
                                      ? ApplicationName
                                      : DataProtectionPurpose )
               .SingleInstance();

        builder.RegisterType<J4JProtection>()
               .As<IJ4JProtection>()
               .SingleInstance();

        builder.Register( c =>
                {
                    var provider = c.Resolve<IDataProtectionProvider>();

                    return new J4JProtection( provider, DataProtectionPurpose );
                } )
               .As<IJ4JProtection>()
               .SingleInstance();
    }

    private void CoreCommandLineDependencyInjection(HostBuilderContext hbc, ContainerBuilder builder)
    {
        // expose IOptionCollection when J4JCommandLine subsystem is being used
        if (_options != null)
            builder.Register(_ => _options)
                   .AsSelf()
                   .SingleInstance();
    }

    private void CoreHostDependencyInjection(HostBuilderContext hbc, ContainerBuilder builder)
    {
        // register the IJ4JHost which will get built
        if (RegisterJ4JHost)
            builder.Register(_ => Host!)
                   .As<IJ4JHost>()
                   .SingleInstance();
    }

    private void CoreLoggingDependencyInjection(HostBuilderContext hbc, ContainerBuilder builder)
    {
        if (RuntimeLoggerInitializer == null)
            return;

        builder.Register(_ => RuntimeLoggerInitializer.Invoke(hbc.Configuration, this))
            .AsImplementedInterfaces()
            .SingleInstance();

        builder.Register( c =>
                {
                    var seriLogger = c.Resolve<ILogger>();

                    return new LoggerFactory()
                       .AddSerilog( seriLogger );
                } )
               .As<ILoggerFactory>()
               .SingleInstance();

        // somehow, this is enough to get ILoggerFactory called to create the instance
        builder.RegisterGeneric( typeof( Logger<> ) )
               .As( typeof( ILogger<> ) );
    }

    private void CoreDataProtectionServices( HostBuilderContext hbc, IServiceCollection services )
    {
        services.AddDataProtection();
    }
}