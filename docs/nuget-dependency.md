# Dependency Injection

**There are breaking changes in this release.** Please re-review the [github documentation](https://github.com/markolbert/ProgrammingUtilities).

Provides a customized `IHostBuilder`-based system for producing an `IHost` instance which contain a number of useful services. These include:

- logging (via [J4JLogger](https://github.com/markolbert/J4JLogging))
- application and user configuration file management
- simple data protection (i.e., encryption and decryption of strings, via `Microsoft.AspNetCore.DataProtection`)
- command line parsing (via [J4JCommandLine](https://github.com/markolbert/J4JCommandLine))
- dependency injection (via [Autofac](https://autofac.org/))

This assembly targets Net5 and has nullability enabled.

This example is derived from my [GeoProcessor](https://github.com/markolbert/GeoProcessor) console app. However, it's been simplified to illustrate certain concepts more clearly and therefore will not work "as written".

```csharp
internal class Program
{
    internal static IHost? Host { get; private set; }

    private static readonly CancellationToken _cancellationToken = new();
    private static IJ4JLogger? _buildLogger;

    private static async Task Main( string[] args )
    {
        var hostConfig = new J4JHostConfiguration()
            .ApplicationName( "GeoProcessor" )
            .Publisher( "J4JSoftware" )
            .OperatingSystem( OSNames.Windows )
            .AddApplicationConfigurationFile("appConfig.json")
            .AddUserConfigurationFile("userConfig.json")
            .AddConfigurationInitializers(SetupConfiguration)
            .LoggerInitializer( SetupLogging )
            .FilePathTrimmer(FilePathTrimmer)
            .OptionsInitializer( SetupOptions )
            .AddDependencyInjectionInitializers( SetupDependencyInjection )
            .AddServicesInitializers( SetupServices );

        _buildLogger = hostConfig.Logger;
        var builder = hostConfig.CreateHostBuilder();
        Host = builder.Build();

        await Host!.RunAsync( _cancellationToken );
    }
}
```

This example Main program shows the basic outline of how to use `J4JConfigurationHost`. You create an instance, call various extension methods on it, check to see if all the basic requirements are satisfied, create the builder by calling `CreateHostBuilder()`, make sure the builder got built, and then call `Build()` on the builder.

Please note, however, that we store the build-time logger, contained in `J4JConfigurationHost`'s `Logger` property, in a field variable. This is done so we can reference it in some of the configuration actions we've defined below. That lets the configuration actions report on issues and problems when they execute when the `IHost` instance gets built.
