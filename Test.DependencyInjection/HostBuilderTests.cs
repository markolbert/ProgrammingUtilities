using System;
using System.IO;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;

#pragma warning disable 8618

namespace Test.DependencyInjection;

public class HostBuilderTests
{
    private class OptionsTest
    {
        public bool Switch { get; set; }
        public string Text { get; set; }
    }

    [ Fact ]
    public void FailNotConfigured()
    {
        var config = new J4JHostConfiguration( AppEnvironment.Console );
        config.MissingRequirements.Should()
              .Be( J4JHostRequirements.ApplicationName | J4JHostRequirements.Publisher );

        var host = config.Build();
        host.Should().BeNull();
    }

    [ Fact ]
    public void SucceedConfigured()
    {
        var config = new J4JHostConfiguration( AppEnvironment.Console )
                    .ApplicationName( "Test" )
                    .Publisher( "J4JSoftware" );

        config.MissingRequirements.Should().Be( J4JHostRequirements.AllMet );

        BuildHost( config );
    }

    [ Fact ]
    public void LoggerTest()
    {
        var config = new J4JHostConfiguration( AppEnvironment.Console )
                    .ApplicationName( "Test" )
                    .Publisher( "J4JSoftware" )
                    .LoggerInitializer( config_logger );

        config.MissingRequirements.Should().Be( J4JHostRequirements.AllMet );

        var host = BuildHost( config );

        var logger = host.Services.GetRequiredService<IJ4JLogger>();
        logger.SetLoggedType( GetType() );
        logger.Fatal( "This is a test fatal event" );

        static void config_logger( IConfiguration buildConfig, J4JHostConfiguration hostConfig, J4JLoggerConfiguration loggerConfig )
        {
            loggerConfig.SerilogConfiguration
                        .WriteTo
                        .Debug( outputTemplate: loggerConfig.GetOutputTemplate( true ) );
        }
    }

    [ Fact ]
    public void LoggerJSONTest()
    {
        var config = new J4JHostConfiguration( AppEnvironment.Console )
                    .ApplicationName( "Test" )
                    .Publisher( "J4JSoftware" )
                    .AddConfigurationInitializers( config_config );

        config.LoggerInitializer( config_logger );

        var host = BuildHost( config );

        var logger = host.Services.GetRequiredService<IJ4JLogger>();
        logger.SetLoggedType( GetType() );
        logger.Fatal( "This is a test fatal event" );

        void config_config( IConfigurationBuilder configBuilder )
        {
            configBuilder.AddJsonFile( Path.Combine( Environment.CurrentDirectory, "appConfig.json" ), false );
        }

        void config_logger( IConfiguration buildConfig, J4JHostConfiguration hostConfig, J4JLoggerConfiguration loggerConfig )
        {
            loggerConfig.SerilogConfiguration
                        .ReadFrom
                        .Configuration( buildConfig );
        }
    }

    [ Fact ]
    public void CommandLineParsing()
    {
        var config = new J4JHostConfiguration( AppEnvironment.Console )
                    .ApplicationName( "Test" )
                    .Publisher( "J4JSoftware" );

        config.AddCommandLineProcessing( CommandLineOperatingSystems.Windows )
              .OptionsInitializer( define_options );

        var host = BuildHost( config );
        host.Should().NotBeNull();

        host.CommandLineSource.Should().NotBeNull();
        host.CommandLineSource!.SetCommandLine( "/s /t \"hello\"" );

        var optConfig = host.Services.GetRequiredService<IConfiguration>();
        optConfig.Should().NotBeNull();

        var cmdOptions = optConfig.Get<OptionsTest>();
        cmdOptions.Should().NotBeNull();

        cmdOptions.Switch.Should().BeTrue();
        cmdOptions.Text.Should().Be( "hello" );

        static void define_options( OptionCollection options )
        {
            options.Bind<OptionsTest, bool>( x => x.Switch, "s" );
            options.Bind<OptionsTest, string>( x => x.Text, "t" );
        }
    }

    [ Fact ]
    public void Protection()
    {
        var config = new J4JHostConfiguration( AppEnvironment.Console )
                    .ApplicationName( "Test" )
                    .Publisher( "J4JSoftware" );

        var host = BuildHost( config );

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

    private static IJ4JHost BuildHost( J4JHostConfiguration config )
    {
        config.MissingRequirements.Should().Be( J4JHostRequirements.AllMet );

        var host = config.Build();
        host.Should().NotBeNull();

        host!.Services.GetRequiredService<IConfiguration>().Should().NotBeNull();
        host.Services.GetRequiredService<IJ4JLogger>().Should().NotBeNull();

        return host;
    }
}