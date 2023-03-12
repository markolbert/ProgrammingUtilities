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
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.DependencyInjection.host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace J4JSoftware.DependencyInjection;

public static class J4JHostConfigurationExtensions
{
    private record CommandLinePath(string Path, bool IsRequired, bool ReloadOnChange);

    public static string ToText( this J4JHostRequirements requirements )
    {
        var sb = new StringBuilder();

        if( ( requirements & J4JHostRequirements.ApplicationName ) == J4JHostRequirements.ApplicationName )
            sb.Append( "Application Name, " );

        if ( ( requirements & J4JHostRequirements.Publisher ) == J4JHostRequirements.Publisher )
            sb.Append( "Publisher" );

        var retVal = sb.ToString().Trim();

        return retVal[ ^1 ] == ',' ? retVal[ 0..^1 ] : retVal;
    }

    public static J4JHostConfiguration Publisher( this J4JHostConfiguration config, string publisher )
    {
        if (string.IsNullOrEmpty(publisher))
            config.BuildLogger.Error("The publisher name cannot be empty");
        else config.Publisher = publisher;

        return config;
    }

    public static J4JHostConfiguration ApplicationName( this J4JHostConfiguration config, string appName )
    {
        if (string.IsNullOrEmpty(appName))
            config.BuildLogger.Error("The application name cannot be empty");
        else config.ApplicationName = appName;
        
        return config;
    }

    public static J4JHostConfiguration DataProtectionPurpose( this J4JHostConfiguration config, string purpose )
    {
        config.DataProtectionPurpose = purpose;
        return config;
    }

    public static J4JHostConfiguration FileSystemCaseSensitivity( this J4JHostConfiguration config, bool caseSensitivity )
    {
        config.FileSystemCaseSensitivity = caseSensitivity;
        return config;
    }

    public static J4JHostConfiguration CommandLineTextComparison( this J4JHostConfiguration config,
        StringComparison? comparison )
    {
        if( comparison.HasValue )
            config.CommandLineTextComparison = comparison.Value;
        else config.ResetCommandLineTextComparison();

        return config;
    }

    public static J4JHostConfiguration AddApplicationConfigurationFile(
        this J4JHostConfiguration config,
        string filePath,
        IEnumerable<string>? folders = null,
        bool optional = true,
        bool reloadOnChange = false
    )
    {
        folders ??= FileFolders.Default;

        if( !FileExtensions.ValidateFilePath( filePath, out var revisedPath, folders: folders ) && !optional )
        {
            config.BuildLogger.Fatal("Could not locate required application configuration file '{0}'", filePath);

            throw new J4JDependencyInjectionException(
                $"Could not locate required application configuration file '{filePath}'",
                config );
        }

        // if we couldn't find the file but it's optional, revert to the original path
        if( string.IsNullOrEmpty( revisedPath ) )
        {
            config.BuildLogger.Error("Could not locate optional application configuration file '{0}'", filePath);
            revisedPath = filePath;
        }

        config.ApplicationConfigurationFiles.Add( new ConfigurationFile( config,
                                                                         ConfigurationFileType.Application,
                                                                         revisedPath,
                                                                         optional,
                                                                         reloadOnChange ) );

        return config;
    }

    public static J4JHostConfiguration AddUserConfigurationFile( this J4JHostConfiguration config,
        string filePath,
        bool optional = true,
        bool reloadOnChange = false )
    {
        config.UserConfigurationFiles.Add(
            new ConfigurationFile( config, ConfigurationFileType.User, filePath, optional, reloadOnChange ) );
        return config;
    }

    public static J4JHostConfiguration AddEnvironmentInitializers( this J4JHostConfiguration config,
        params Action<HostBuilderContext,
            IConfigurationBuilder>[] initializers )
    {
        config.EnvironmentInitializers.AddRange( initializers );
        return config;
    }

    public static J4JHostConfiguration AddConfigurationInitializers( this J4JHostConfiguration config,
        params Action<IConfigurationBuilder>[]
            initializers )
    {
        config.ConfigurationInitializers.AddRange( initializers );
        return config;
    }

    public static J4JHostConfiguration AddDependencyInjectionInitializers( this J4JHostConfiguration config,
        params Action<HostBuilderContext,
            ContainerBuilder>[] initializers )
    {
        config.DependencyInjectionInitializers.AddRange( initializers );
        return config;
    }

    public static J4JHostConfiguration AddServicesInitializers( this J4JHostConfiguration config,
        params Action<HostBuilderContext,
            IServiceCollection>[] initializers )
    {
        config.ServicesInitializers.AddRange( initializers );
        return config;
    }

    public static J4JHostConfiguration LoggerInitializer( this J4JHostConfiguration config,
        Func<IConfiguration, J4JHostConfiguration, ILogger>
            initializer )
    {
        config.RuntimeLoggerInitializer = initializer;
        return config;
    }

    public static J4JCommandLineConfiguration AddCommandLineProcessing( this J4JHostConfiguration config,
        CommandLineOperatingSystems? operatingSystem = null )
    {
        operatingSystem ??= Environment.OSVersion.Platform switch
        {
            PlatformID.MacOSX => CommandLineOperatingSystems.Linux,
            PlatformID.Unix => CommandLineOperatingSystems.Linux,
            PlatformID.Win32NT => CommandLineOperatingSystems.Windows,
            PlatformID.Win32S => CommandLineOperatingSystems.Windows,
            PlatformID.Win32Windows => CommandLineOperatingSystems.Windows,
            PlatformID.WinCE => CommandLineOperatingSystems.Windows,
            PlatformID.Xbox => CommandLineOperatingSystems.Windows,
            _ => CommandLineOperatingSystems.Linux
        };

        config.CommandLineConfiguration = new J4JCommandLineConfiguration( config, operatingSystem.Value );

        return config.CommandLineConfiguration;
    }

    public static J4JCommandLineConfiguration LexicalElements( this J4JCommandLineConfiguration config,
        ILexicalElements tokens )
    {
        config.LexicalElements = tokens;
        return config;
    }

    public static J4JCommandLineConfiguration TextConverters( this J4JCommandLineConfiguration config,
        ITextConverters converters )
    {
        config.TextConverters = converters;
        return config;
    }

    public static J4JCommandLineConfiguration OptionsGenerator( this J4JCommandLineConfiguration config,
        IOptionsGenerator generator )
    {
        config.OptionsGenerator = generator;
        return config;
    }

    public static J4JCommandLineConfiguration TokenCleaners( this J4JCommandLineConfiguration config,
        params ICleanupTokens[] tokenCleaners )
    {
        config.CleanupProcessors.AddRange( tokenCleaners );
        return config;
    }

    public static J4JCommandLineConfiguration OptionsInitializer(
        this J4JCommandLineConfiguration config,
        Action<OptionCollection> initializer
    )
    {
        config.OptionsInitializer = initializer;
        config.HostConfiguration.ConfigurationInitializers.Add( config.HostConfiguration.SetupCommandLineParsing );

        return config;
    }

    public static J4JCommandLineConfiguration ConfigurationFileKeys(
        this J4JCommandLineConfiguration config,
        bool required,
        bool reloadOnChange,
        params string[] optionKeys
    )
    {
        config.ConfigurationFileKeys = new ConfigurationFileKeys( required, reloadOnChange, optionKeys );
        return config;
    }

    public static IJ4JHost? Build( this J4JHostConfiguration config )
    {
        if( config.MissingRequirements != J4JHostRequirements.AllMet )
        {
            config.BuildLogger.Fatal("J4JHostConfiguration: some or all requirements not met");
            return null;
        }

        // this next call should never fail, but...
        if (!config.TryGetUserConfigurationFolder(out var userConfigFolder))
        {
            config.BuildLogger.Fatal("J4JHostConfiguration: user configuration folder is undefined");
            return null;
        }

        var hostBuilder = new HostBuilder()
           .UseServiceProviderFactory( new AutofacServiceProviderFactory() );

        var cmdLinePaths = GetCommandLinePaths(config);

        if (cmdLinePaths.Any())
        {
            config.ApplicationConfigurationFiles.Clear();

            foreach( var cmdLinePath in cmdLinePaths)
            {
                config.ApplicationConfigurationFiles.Add(new ConfigurationFile(config,
                                                                            ConfigurationFileType.Application,
                                                                            cmdLinePath.Path,
                                                                            cmdLinePath.IsRequired,
                                                                            cmdLinePath.ReloadOnChange));
            }
        }

        foreach ( var configurator in config.EnvironmentInitializers )
        {
            hostBuilder.ConfigureAppConfiguration( configurator );
        }

        foreach ( var configurator in config.ConfigurationInitializers )
        {
            hostBuilder.ConfigureHostConfiguration( configurator );
        }

        foreach ( var configurator in config.DependencyInjectionInitializers )
        {
            hostBuilder.ConfigureContainer( configurator );
        }

        foreach ( var configurator in config.ServicesInitializers )
        {
            hostBuilder.ConfigureServices( configurator );
        }

        config.Host = new J4JHost( hostBuilder.Build(), config.AppEnvironment )
        {
            ApplicationName = config.ApplicationName,
            CommandLineTextComparison = config.CommandLineTextComparison,
            CommandLineLexicalElements = config.CommandLineConfiguration?.LexicalElements,
            CommandLineSource = config.CommandLineSource,
            FileSystemIsCaseSensitive = config.FileSystemCaseSensitivity,
            Publisher = config.Publisher,
            ApplicationConfigurationFolder = config.ApplicationConfigurationFolder,
            ApplicationConfigurationFiles = config.ApplicationConfigurationFiles.Select( x => x.FilePath ).ToList(),
            UserConfigurationFolder = userConfigFolder!,
            UserConfigurationFiles = config.UserConfigurationFiles.Select( x => x.FilePath ).ToList(),
        };

        return config.Host;
    }

    // we have to parse the command line from scratch so we don't interfere with
    // the IConfiguration subsystem, which doesn't like parsing command lines twice
    private static List<CommandLinePath> GetCommandLinePaths( J4JHostConfiguration hostConfig )
    {
        var retVal = new List<CommandLinePath>();

        if( hostConfig.CommandLineConfiguration is not {} cmdConfig 
           || cmdConfig.OptionsInitializer == null 
           || cmdConfig.ConfigurationFileKeys is not {} fileKeys )
            return retVal;

        // we create default values for missing required parameters, sometimes based on the 
        // type of operating system specified
        var textConverters = cmdConfig.TextConverters ?? new TextConverters();

        var optionCollection = new OptionCollection( hostConfig.CommandLineTextComparison,
                                            textConverters,
                                            hostConfig.BuildLogger );

        cmdConfig.OptionsInitializer( optionCollection );
        optionCollection.FinishConfiguration();

        var optionsGenerator = new OptionsGenerator( optionCollection, hostConfig.CommandLineTextComparison, hostConfig.BuildLogger );

        var lexicalElements = cmdConfig.OperatingSystem switch
        {
            CommandLineOperatingSystems.Windows =>
                new WindowsLexicalElements(hostConfig.BuildLogger),
            CommandLineOperatingSystems.Linux =>
                new LinuxLexicalElements(hostConfig.BuildLogger),
            _ => (LexicalElements?) null
        };

        if( lexicalElements == null )
            return retVal;

        var parsingTable = new ParsingTable( optionsGenerator );
        var tokenizer = new Tokenizer( lexicalElements, hostConfig.BuildLogger );

        var parser = new Parser( optionCollection, parsingTable, tokenizer, hostConfig.BuildLogger );

        var pathComparer = hostConfig.FileSystemCaseSensitivity
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        // grab the command line
        // we don't want the name of the executable so we need to remove it
        // it's the first argument in what gets returned by Environment.GetCommandLineArgs()
        var temp = Environment.GetCommandLineArgs();
        var cmdLine = Environment.CommandLine;

        cmdLine = cmdLine.IndexOf(temp[0], pathComparer) == 0
            ? cmdLine.Replace(temp[0], string.Empty)
            : cmdLine;

        if( !parser.Parse( cmdLine ) )
            return retVal;

        foreach( var cmdKey in fileKeys.CommandLineKeys )
        {
            if( optionCollection[cmdKey] is not {} curOption )
                continue;

            if( !curOption.GetValue(out var curPath ))
                continue;

            switch( curPath )
            {
                case string singlePath:
                    var newCmdLinePath = new CommandLinePath( singlePath, fileKeys.Required, fileKeys.ReloadOnChange );
                    if( !retVal.Any( x => string.Equals(x.Path, newCmdLinePath.Path, pathComparer ) ) )
                        retVal.Add( newCmdLinePath );

                    break;

                case IEnumerable<string> multiplePaths:
                    foreach( var filePath in multiplePaths )
                    {
                        var newCmdLinePath2 = new CommandLinePath(filePath, fileKeys.Required, fileKeys.ReloadOnChange);
                        if (!retVal.Any(x => string.Equals(x.Path, newCmdLinePath2.Path, pathComparer)))
                            retVal.Add(newCmdLinePath2);
                    }

                    break;

                default:
                    hostConfig.BuildLogger.Warning(
                        "Command line option for '{0}' is not a string or an IEnumerable<string>",
                        cmdKey );
                    break;
            }
        }

        return retVal;
    }
}