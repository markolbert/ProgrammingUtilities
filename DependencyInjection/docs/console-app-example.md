# J4JHost: Console Application Example

This example is derived from my [WpFormsSurveyProcessor](https://github.com/markolbert/WPFormsSurveyProcessor/tree/master/WPFormsSurveyProcessor) console app. It uses both the `IJ4JHost` and `J4JDeusEx` APIs.

```csharp
internal class Program
{
    static void Main( string[] args )
    {
        var deusEx = new DeusEx();

        if( !deusEx.Initialize() )
        {
            J4JDeusEx.Logger?.Fatal("Could not initialize application");
            Environment.ExitCode = 1;
            return;
        }

        var service = GetService();

        var cancellationTokenSrc = new CancellationTokenSource();
        service.StartAsync( cancellationTokenSrc.Token );
    }

    // remaining details omitted for clarity
}

// this class is marked as partial but that's simply to keep the codebase clean
internal partial class DeusEx : J4JDeusExHosted
{
    protected override J4JHostConfiguration? GetHostConfiguration()
    {
        var hostConfig = new J4JHostConfiguration( AppEnvironment.Console )
                        .ApplicationName( "WpFormsSurveyProcessor" )
                        .Publisher( "Jump for Joy Software" )
                        .LoggerInitializer( ConfigureLogging )
                        .AddDependencyInjectionInitializers( ConfigureDependencyInjection )
                        .FilePathTrimmer( FilePathTrimmer );

        var cmdLineConfig = hostConfig.AddCommandLineProcessing( CommandLineOperatingSystems.Windows )
                                      .OptionsInitializer( SetCommandLineConfiguration )
                                      .ConfigurationFileKeys( true, false, "c", "config" );

        return hostConfig;
    }

    // remaining details omitted for clarity
}
```

This example shows the basic pattern for using the two APIs:

- derive a class from `J4JDeusExHosted`;
- implement the `GetHostConfiguration()` method, in which you construct an instance of `J4JHostConfiguration`;
- create an instance of your derived class and call it's `Initialize()` method.

If everything worked you're ready to begin processing. If something went wrong, you exit (and check the logs and/or crash file).
