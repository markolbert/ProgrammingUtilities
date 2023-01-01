# J4JHost: Windows Desktop Application Example

This example is derived from my [GPSLocator](https://github.com/markolbert/WPFormsSurveyProcessor/tree/master/WPFormsSurveyProcessor) Windows Application (v3, aka WinApp3). It uses both the `IJ4JHost` and `J4JDeusEx` APIs.

## Sandboxed Environment

As a Windows Application v3 app, `GPSLocator` is sandboxed: it does not have unfettered access to the file system.

```csharp
public partial class App : Application
{
    private readonly IJ4JLogger _logger;

    public App()
    {
        this.InitializeComponent();

        this.UnhandledException += App_UnhandledException;

        var deusEx = new GPSLocatorDeusEx();
        if ( !deusEx.Initialize() )
            throw new J4JDeusExException( "Couldn't configure J4JDeusEx object" );

        _logger = J4JDeusEx.ServiceProvider.GetRequiredService<IJ4JLogger>();
    }

    private void App_UnhandledException( object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e )
    {
        J4JDeusEx.OutputFatalMessage($"Unhandled exception: {e.GetType().Name}", null);
        J4JDeusEx.OutputFatalMessage( $"{e.Message}", null );
    }
}

internal class GPSLocatorDeusEx : J4JDeusExWinApp
{
    private const string Publisher = "J4JSoftware";
    private const string ApplicationName = "GpsLocator";

    protected override J4JHostConfiguration? GetHostConfiguration()
    {
        return new J4JWinAppHostConfiguration()
                        .Publisher(Publisher)
                        .ApplicationName(ApplicationName)
                        .LoggerInitializer(InitializeLogger)
                        .AddNetEventSinkToLogger()
                        .AddDependencyInjectionInitializers(SetupDependencyInjection)
                        .AddServicesInitializers(InitializeServices)
                        .AddUserConfigurationFile("userConfig.json", optional: true)
                        .FilePathTrimmer(FilePathTrimmer)
            as J4JWinAppHostConfiguration;
    }

    // remaining details omitted for clarity
}
```

The basic pattern for using the two APIs in a sandboxed environment is this:

- derive a class from `J4JDeusExWinApp`;
- implement the `GetHostConfiguration()` method, in which you construct an instance of `J4JWinAppHostConfiguration`. You use it, rather than `J4JHostConfiguration`, because `J4JWinAppHostConfiguration` takes care of the sandboxing that Windows enforces on WinApp3 applications.;
- create an instance of your derived class and call it's `Initialize()` method.

If everything worked you're ready to begin processing. If something went wrong, an exception is thrown (and you can check the logs and/or crash file).

The example also shows how the crash file component of `J4JDeusEx` can be used. We implement a custom handler for unhandled exceptions, and write the exception information to the crash file using `J4JDeusEx.OutputFatalMessage()`.

## Non-sandboxed Environment

The only change you have to make in a non-sandboxed (desktop) environment is to derive your DeusEx class from `J4JDeusExHosted` rather than `J4JDeusExWinApp`. After that, the rest of the code is the same.
