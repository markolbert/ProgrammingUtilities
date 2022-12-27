# Console Application Example

This example is derived from my [GeoProcessor](https://github.com/markolbert/GeoProcessor) console app. However, it's been simplified to illustrate certain concepts more clearly and therefore will not work "as written".

```csharp
internal class Program
{
    internal static IHost? Host { get; private set; }

    private static readonly CancellationToken _cancellationToken = new();

    private static IJ4JLogger? _buildLogger;

    private static async Task Main( string[] args )
    {
        var hostConfig = new J4JHostConfiguration( AppEnvironment.Console )
                        .ApplicationName( "GeoProcessor" )
                        .Publisher( "J4JSoftware" )
                        .AddApplicationConfigurationFile( "appConfig.json" )
                        .AddUserConfigurationFile( "userConfig.json" )
                        .AddConfigurationInitializers( SetupConfiguration )
                        .LoggerInitializer( SetupLogging )
                        .FilePathTrimmer( FilePathTrimmer )
                        .AddDependencyInjectionInitializers( SetupDependencyInjection )
                        .AddServicesInitializers( SetupServices );

        hostConfig.AddCommandLineProcessing( CommandLineOperatingSystems.Windows )
                    .OptionsInitializer( SetupOptions );

        _buildLogger = hostConfig.Logger;

        if( hostConfig.MissingRequirements != J4JHostRequirements.AllMet )
        {
            Console.WriteLine(
                $"Could not create IHost. The following requirements were not met: {hostConfig.MissingRequirements.ToText()}" );
            Environment.ExitCode = -1;

            return;
        }

        Host = hostConfig.Build();

        if( Host == null )
        {
            Console.WriteLine( "Failed to build host" );
            Environment.ExitCode = -1;

            return;
        }

        await Host!.RunAsync( _cancellationToken );
    }
}
```

This example Main program shows the basic outline of how to use `J4JConfigurationHost`. You create an instance, call various extension methods on it, check to see if all the basic requirements are satisfied, create the builder by calling `CreateHostBuilder()`, make sure the builder got built, and then call `Build()` on the builder.

Please note, however, that we store the build-time logger, contained in `J4JConfigurationHost`'s `Logger` property, in a field variable. This is done so we can reference it in some of the configuration actions we've defined below. That lets the configuration actions report on issues and problems when they execute when the `IHost` instance gets built.

I won't go through all the configuration methods (check the [GeoProcessor documentation](https://github.com/markolbert/GeoProcessor) for details) but will showcase a few of them here.

The first one demonstrates how and why you might want to provide an `IConfiguration` initializer even though you've already specified, through other extension methods, the names of your application and user config files.

```csharp
private static void SetupConfiguration( IConfigurationBuilder builder )
{
    builder.AddUserSecrets<AppConfig>();
}
```

The next one shows how to use the file-based configuration capability of `Serilog` to configure `J4JLogger` (which wraps a `Serilog` logger to add functionality):

```csharp
private static void SetupLogging( IConfiguration config, J4JLoggerConfiguration loggerConfig )
    => loggerConfig.SerilogConfiguration.ReadFrom.Configuration( config );
```

Configuring how the `J4JCommandLine` subsystem parses the command line is straightforward:

```csharp
private static void SetupOptions( IOptionCollection options )
{
    options.Bind<AppConfig, string>(x => x.InputFile.FilePath, "i", "inputFile");
    options.Bind<AppConfig, string>(x => x.DefaultRouteName, "n", "defaultName");
    options.Bind<AppConfig, string>(x => x.OutputFile.FilePath, "o", "outputFile");
    options.Bind<AppConfig, ExportType>(x => x.ExportType, "t", "outputType");
    options.Bind<AppConfig, bool>(x => x.StoreAPIKey, "k", "storeApiKey");
    options.Bind<AppConfig, bool>(x => x.RunInteractive, "r", "runInteractive");
    options.Bind<AppConfig, ProcessorType>(x => x.ProcessorType, "p", "snapProcessor");
}
```

As is adding services to be managed by the `IHost` you'll be creating. Note that this example shows how to select different functionalities for the console app depending upon command line parameters. It's done by accessing the `IConfiguration` subsystem for the command line option value and adding the appropriate service:

```csharp
private static void SetupServices( HostBuilderContext hbc, IServiceCollection services )
{
    var config = hbc.Configuration.Get<AppConfig>();

    if( config == null )
    {
        _buildLogger?.Fatal( "Cannot get AppConfig from IConfiguration" );

        return;
    }

    if( config.StoreAPIKey )
        services.AddHostedService<StoreKeyApp>();
    else
        services.AddHostedService<RouteApp>();
}
```
