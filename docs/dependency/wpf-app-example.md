# WPF Application Example

Using the library with WPF applications is a little more involved than with console apps. That's because, in addition to creating an `IHost` instance you must do so within a class which can be created and accessed by the various UI elements.

That class is traditionally called `ViewModelLocator`, at least when you're using dependency injection techniques as part of an MVVM (model-view-viewmodel) architecture. 

The example here is from my [GeoProcessor](https://github.com/markolbert/GeoProcessor) WPF app. It's been simplified to highlight certain points, so please remember it won't work "as is".

```csharp
public class ViewModelLocator
{
    private readonly IHost _host;

    private IJ4JLogger? _buildLogger;

    public ViewModelLocator()
    {
        _host = CreateHost();
    }

    public MainVM MainVM => _host!.Services.GetRequiredService<MainVM>();
    public ProcessorVM ProcessorVM => _host!.Services.GetRequiredService<ProcessorVM>();
    public OptionsVM OptionsVM => _host!.Services.GetRequiredService<OptionsVM>();
    public RouteDisplayVM RouteDisplayVM => _host!.Services.GetRequiredService<RouteDisplayVM>();
    public RouteEnginesVM RouteEnginesVM => _host!.Services.GetRequiredService<RouteEnginesVM>();
}
```

An instance of `ViewModelLocator` is defined in the app's `App.xaml` file:

```xml
<Application x:Class="J4JSoftware.GeoProcessor.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:J4JSoftware.GeoProcessor"
             Startup="Application_Startup"
             Exit="Application_Exit">

    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary>
                    <local:ViewModelLocator x:Key="ViewModelLocator" />
                    <local:TextToDouble x:Key="textToDouble" />
                </ResourceDictionary>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

This ensures a single instance of `ViewModelLocator` will be available as a resource throughout the app.

Of course, configuring the `IHost` that manages providing the various view models is key. Here's what the `CreateHost()` method looks like:

```csharp
private IHost CreateHost()
{
    var hostConfig = new J4JHostConfiguration()
        .OperatingSystem( OSNames.Windows )
        .Publisher( "J4JSoftware" )
        .ApplicationName( "GeoProcessor" )
        .AddApplicationConfigurationFile( AppConfigFile )
        .AddUserConfigurationFile( UserConfigFile )
        .AddConfigurationInitializers( ConfigurationInitializer )
        .LoggerInitializer( LoggerInitializer )
        .AddNetEventSinkToLogger()
        .FilePathTrimmer( FilePathTrimmer )
        .AddDependencyInjectionInitializers( SetupDependencyInjection );

    _buildLogger = hostConfig.Logger;

    if( hostConfig.MissingRequirements != J4JHostRequirements.AllMet )
        throw new ArgumentException(
            $"Could not create IHost. The following requirements were not met: {hostConfig.MissingRequirements.ToText()}" );

    var builder = hostConfig.CreateHostBuilder();

    if( builder == null )
        throw new ArgumentException( "Failed to create host builder." );

    var retVal = builder.Build();

    if( retVal == null )
        throw new ArgumentException( "Failed to build host" );

    return retVal;
}
```

This shows the basic outline of how to use `J4JConfigurationHost` in a WPF app. You create an instance, call various extension methods on it, check to see if all the basic requirements are satisfied, create the builder by calling `CreateHostBuilder()`, make sure the builder got built, and then call `Build()` on the builder.

Note that, unlike with the console example, if something goes wrong we don't log the error. We just terminate the app. There are likely more elegant ways of detecting startup problems, but this is meant just as an example.

We store the build-time logger, contained in `J4JConfigurationHost`'s `Logger` property, in a field variable. This is done so we can reference it in some of the configuration actions we've defined below. That lets the configuration actions report on issues and problems when they execute when the `IHost` instance gets built.

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

The viewmodels we'll be using are registered via dependency injectdion::

```csharp
private void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
{
    builder.Register( c =>
        {
            var retVal = hbc.Configuration.Get<UserConfig>();

            var protection = c.Resolve<IJ4JProtection>();

            foreach( var kvp in retVal.APIKeys )
            {
                kvp.Value.Initialize( protection );
            }

            return retVal;
        } )
        .AsImplementedInterfaces()
        .SingleInstance();

    builder.Register( c => hbc.Configuration.Get<AppConfig>() )
        .AsSelf()
        .AsImplementedInterfaces()
        .SingleInstance();

    builder.RegisterType<MainVM>()
        .AsSelf();

    builder.RegisterType<OptionsVM>()
        .AsSelf();

    builder.RegisterType<RouteDisplayVM>()
        .AsSelf();

    builder.RegisterType<RouteEnginesVM>()
        .AsSelf();

    builder.RegisterType<ProcessorVM>()
        .AsSelf();

    builder.RegisterModule<AutofacGeoProcessorModule>();
}
```
