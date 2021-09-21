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
using System.Reflection;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Events;

namespace J4JSoftware.DependencyInjection
{
    public static class J4JHostConfigurationExtensions
    {
        public static string ToText( this J4JHostRequirements requirements )
        {
            var sb = new StringBuilder();

            if( (requirements & J4JHostRequirements.ApplicationName) == J4JHostRequirements.ApplicationName )
                sb.Append( "Application Name, " );

            if ((requirements & J4JHostRequirements.Publisher) == J4JHostRequirements.Publisher)
                sb.Append("Publisher");

            var retVal = sb.ToString().Trim();

            return retVal[ ^0 ] == ',' ? retVal[ 0..^1 ] : retVal;
        }

        public static J4JHostConfiguration Publisher( this J4JHostConfiguration config, string publisher )
        {
            config.Publisher = publisher;
            return config;
        }

        public static J4JHostConfiguration ApplicationName(this J4JHostConfiguration config, string name)
        {
            config.ApplicationName= name;
            return config;
        }

        public static J4JHostConfiguration DataProtectionPurpose(this J4JHostConfiguration config, string purpose)
        {
            config.DataProtectionPurpose = purpose;
            return config;
        }

        public static J4JHostConfiguration CommandLineOperatingSystem(
            this J4JHostConfiguration config,
            CommandLineOperatingSystems os
        )
        {
            config.OperatingSystem = os;
            return config;
        }

        public static J4JHostConfiguration CaseSensitiveFileSystem( this J4JHostConfiguration config )
        {
            config.CaseSensitiveFileSystem = true;
            return config;
        }

        public static J4JHostConfiguration CaseInsensitiveFileSystem(this J4JHostConfiguration config)
        {
            config.CaseSensitiveFileSystem = false;
            return config;
        }

        public static J4JHostConfiguration AddApplicationConfigurationFile( 
            this J4JHostConfiguration config,
            string filePath,
            bool optional = true,
            bool reloadOnChange = false
            )
        {
            config.ApplicationConfigurationFiles.Add( new ConfigurationFile( filePath, optional, reloadOnChange ) );
            return config;
        }

        public static J4JHostConfiguration AddUserConfigurationFile(
            this J4JHostConfiguration config,
            string filePath,
            bool optional = true,
            bool reloadOnChange = false
                )
        {
            config.UserConfigurationFiles.Add( new ConfigurationFile( filePath, optional, reloadOnChange ) );
            return config;
        }

        public static J4JHostConfiguration AddEnvironmentInitializers( 
            this J4JHostConfiguration config,
            params Action<HostBuilderContext, IConfigurationBuilder>[] initializers )
        {
            config.EnvironmentInitializers.AddRange( initializers );
            return config;
        }

        public static J4JHostConfiguration AddConfigurationInitializers(
            this J4JHostConfiguration config,
            params Action<IConfigurationBuilder>[] initializers)
        {
            config.ConfigurationInitializers.AddRange(initializers);
            return config;
        }

        public static J4JHostConfiguration AddDependencyInjectionInitializers(
            this J4JHostConfiguration config,
            params Action<HostBuilderContext, ContainerBuilder>[] initializers)
        {
            config.DependencyInjectionInitializers.AddRange(initializers);
            return config;
        }

        public static J4JHostConfiguration AddServicesInitializers(
            this J4JHostConfiguration config,
            params Action<HostBuilderContext, IServiceCollection>[] initializers)
        {
            config.ServicesInitializers.AddRange(initializers);
            return config;
        }

        public static J4JHostConfiguration LoggerInitializer(
            this J4JHostConfiguration config,
            Action<IConfiguration, J4JLoggerConfiguration> initializer )
        {
            config.LoggerInitializer = initializer;
            return config;
        }

        public static J4JHostConfiguration AddNetEventSinkToLogger(
            this J4JHostConfiguration config,
            string? outputTemplate = null,
            LogEventLevel minimumLevel = LogEventLevel.Verbose
        )
        {
            config.NetEventConfiguration = new NetEventConfiguration( outputTemplate, minimumLevel );
            return config;
        }

        public static J4JHostConfiguration FilePathTrimmer(this J4JHostConfiguration config, Func<Type?, string, int, string, string> filePathTrimmer)
        {
            config.FilePathTrimmer = filePathTrimmer;
            return config;
        }

        public static J4JHostConfiguration CommandLineAvailableTokens(
            this J4JHostConfiguration config,
            IAvailableTokens tokens )
        {
            config.CommandLineAvailableTokens = tokens;
            return config;
        }

        public static J4JHostConfiguration CommandLineBindabilityValidator(
            this J4JHostConfiguration config,
            IBindabilityValidator bindabilityValidator )
        {
            config.CommandLineBindabilityValidator = bindabilityValidator;
            return config;
        }

        public static J4JHostConfiguration CommandLineOptionsGenerator(
            this J4JHostConfiguration config,
            IOptionsGenerator generator )
        {
            config.CommandLineOptionsGenerator = generator;
            return config;
        }

        public static J4JHostConfiguration CommandLineTokenCleaners(
            this J4JHostConfiguration config,
            params ICleanupTokens[] tokenCleaners )
        {
            config.CommandLineCleanupProcessors.AddRange( tokenCleaners );
            return config;
        }

        public static J4JHostConfiguration CommandLineOptionsInitializer(
            this J4JHostConfiguration config,
            Action<IOptionCollection> initializer)
        {
            config.CommandLineOptionsInitializer = initializer;
            config.ConfigurationInitializers.Add( config.SetupCommandLineParsing );

            return config;
        }

        public static IHostBuilder? CreateHostBuilder( this J4JHostConfiguration config )
        {
            if( config.MissingRequirements != J4JHostRequirements.AllMet )
                return null;

            var retVal = new HostBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory());

            if( retVal == null )
                return null;

            foreach (var configurator in config.EnvironmentInitializers)
            {
                retVal.ConfigureAppConfiguration(configurator);
            }

            foreach (var configurator in config.ConfigurationInitializers)
            {
                retVal.ConfigureHostConfiguration(configurator);
            }

            foreach (var configurator in config.DependencyInjectionInitializers)
            {
                retVal.ConfigureContainer(configurator);
            }

            foreach (var configurator in config.ServicesInitializers)
            {
                retVal.ConfigureServices(configurator);
            }

            return retVal;
        }
    }
}