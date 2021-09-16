using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Xunit;

namespace Test.DependencyInjection
{
    public class HostBuilderTests
    {
        private class OptionsTest
        {
            public bool Switch { get; set; }
            public string Text { get; set; }
        }

        [Fact]
        public void FailNotConfigured()
        {
            var builder = new J4JHostBuilder();

            builder.Build( out var status, out var missing ).Should().BeNull();
            status.Should().Be( J4JHostBuilder.BuildStatus.NotInitialized );
            missing.Should().Be( J4JHostBuilder.Requirements.AllMissing );
        }

        [Fact]
        public void SucceedConfigured()
        {
            var builder = new J4JHostBuilder()
                .ApplicationName( "Test" )
                .Publisher( "J4JSoftware" )
                .OperatingSystem( OSNames.Windows );

            var host = BuildHost( builder );
        }

        [Fact]
        public void LoggerTest()
        {
            var builder = new J4JHostBuilder()
                .ApplicationName( "Test" )
                .Publisher( "J4JSoftware" )
                .OperatingSystem( OSNames.Windows )
                .SetupLogging( config_logger );

            var host = BuildHost(builder);

            var logger = host.Services.GetRequiredService<IJ4JLogger>();
            logger.SetLoggedType( GetType() );
            logger.Fatal( "This is a test fatal event" );

            void config_logger( J4JLoggerConfiguration loggerConfig )
            {
                loggerConfig.SerilogConfiguration
                    .WriteTo
                    .Debug(outputTemplate: loggerConfig.GetOutputTemplate(true));
            }
        }

        [Fact]
        public void LoggerJSONTest()
        {
            var builder = new J4JHostBuilder()
                .ApplicationName( "Test" )
                .Publisher( "J4JSoftware" )
                .OperatingSystem( OSNames.Windows );

            builder.SetupConfiguration( config_config )
                .SetupLogging( config_logger );

            var host = BuildHost(builder);

            var logger = host.Services.GetRequiredService<IJ4JLogger>();
            logger.SetLoggedType(GetType());
            logger.Fatal("This is a test fatal event");

            void config_config( IConfigurationBuilder configBuilder )
            {
                configBuilder.AddJsonFile( Path.Combine( Environment.CurrentDirectory, "appConfig.json" ), false );
            }

            void config_logger( J4JLoggerConfiguration loggerConfig )
            {
                loggerConfig.SerilogConfiguration
                    .ReadFrom
                    .Configuration( builder!.ConfigurationDuringBuild );
            }
        }

        [Fact]
        public void CommandLineParsing()
        {
            var builder = new J4JHostBuilder()
                .ApplicationName("Test")
                .Publisher("J4JSoftware")
                .OperatingSystem(OSNames.Windows);

            builder.DefineCommandLineOptions( define_options );

            var host = BuildHost( builder );

            var hostInfo = host!.Services.GetRequiredService<J4JHostInfo>();
            hostInfo.Should().NotBeNull();
            hostInfo.CommandLineSource.Should().NotBeNull();

            hostInfo.CommandLineSource!.SetCommandLine( "/s /t \"hello\"" );

            var config = host.Services.GetRequiredService<IConfiguration>();
            config.Should().NotBeNull();

            var cmdOptions = config.Get<OptionsTest>();
            cmdOptions.Should().NotBeNull();

            cmdOptions.Switch.Should().BeTrue();
            cmdOptions.Text.Should().Be( "hello" );

            void define_options( IOptionCollection options )
            {
                options.Bind<OptionsTest, bool>( x => x.Switch, "s" );
                options.Bind<OptionsTest, string>( x => x.Text, "t" );
            }
        }

        [Fact]
        public void Protection()
        {
            var builder = new J4JHostBuilder()
                .ApplicationName("Test")
                .Publisher("J4JSoftware")
                .OperatingSystem(OSNames.Windows);

            var host = BuildHost(builder);

            var protector = host!.Services.GetRequiredService<IJ4JProtection>();
            protector.Should().NotBeNull();

            protector.Protect( "test text", out var encrypted )
                .Should()
                .BeTrue();

            protector.Unprotect( encrypted!, out var decrypted )
                .Should()
                .BeTrue();

            decrypted.Should().Be( "test text" );
        }

        private IHost BuildHost( J4JHostBuilder builder )
        {
            var host = builder.Build(out var status, out var missing);
            host.Should().NotBeNull();

            status.Should().Be(J4JHostBuilder.BuildStatus.Built);
            missing.Should().Be(J4JHostBuilder.Requirements.AllMet);

            host!.Services.GetRequiredService<IConfiguration>().Should().NotBeNull();
            host.Services.GetRequiredService<IJ4JLogger>().Should().NotBeNull();

            var hostInfo = host.Services.GetRequiredService<J4JHostInfo>();
            hostInfo.Should().NotBeNull();

            return host;
        }
    }
}
