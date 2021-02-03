## WPF View Model
Provides a composition root tailored for use in WPF applications, which also sets up
`IJ4JLogger` and provides data encryption/decryption capabilities via 
`Microsoft.AspNetCore.DataProtection`. The dependency injection
capabilities are provided by [Autofac](https://autofac.org/).

This assembly targets Net5 and has nullability enabled.

#### Table of Contents
- J4JViewModelLocator
`J4JViewModelLocator` extends [J4JCompositionRoot](dependency.md) for use in
WPF applications built using the MVVM approach. 
  -  [Properties](#j4jviewmodellocator-properties)
  -  [Example](#j4jviewmodellocator-example)

- [Color Conversion](#color-conversion)

#### J4JViewModelLocator Properties
- the **InDesignMode** property is `true` if the code is running under the 
Visual Studio WPF designer, `false` otherwise. This is important for registering
the correct 'version' of a class used to provide data to the UI. The Visual
Studio designer can show editable previews of UI elements, but generally the data
needed to configure them can't be the same version as is used at run-time.

- the virtual method `RegisterViewModels()` lets you configure a run-time and a
design-time class for the same interface. `J4JViewModelLocator` picks the correct
one to use depending on the environment in which it is called, design-time or
run-time.

- the `ApplicationConfigurationFolder` property is overridden to point at
`AppContext.BaseDirectory` when code is running at design-time. This is the
design-time location of the traditional `Environment.CurrentDirectory` folder
which is the 'home directory' of an application at run-time. The override lets
the Net5 `IConfiguration` system find files that are normally available at 
`Environment.CurrentDirectory` when code is running in design mode.

#### J4JViewModelLocator Example
Here's an example of how to use `RegisterViewModels()`. Note the way the
`IJ4JLogger` system is set up differently at design-time and run-time. That's
required in this example because I didn't want the app configuration files read 
at design-time (frankly, I don't remember why I wanted that :) ):
```csharp
public class CompositionRoot : J4JViewModelLocator<J4JLoggerConfiguration>
{
    public const string AppName = "GeoProcessor";
    public const string AppConfigFile = "appConfig.json";
    public const string UserConfigFile = "userConfig.json";

    public CompositionRoot()
        : base( "J4JSoftware", AppName, "J4JSoftware.GeoProcessor.DataProtection" )
    {
        if( InDesignMode )
        {
            var loggerConfig = new J4JLoggerConfiguration();

            loggerConfig.Channels.Add( new DebugConfig() );

            StaticConfiguredLogging( loggerConfig );
        }
        else
        {
            var provider = new ChannelConfigProvider( "Logging" )
                .AddChannel<ConsoleConfig>( "Channels:Console" )
                .AddChannel<DebugConfig>( "Channels:Debug" );

            ConfigurationBasedLogging( provider );
        }

        Initialize();
    }

    public IMainViewModel MainViewModel => Host!.Services.GetRequiredService<IMainViewModel>();
    public IProcessorViewModel ProcessorViewModel => Host!.Services.GetRequiredService<IProcessorViewModel>();
    public IOptionsViewModel OptionsViewModel => Host!.Services.GetRequiredService<IOptionsViewModel>();
    public IRouteDisplayViewModel RouteDisplayViewModel => Host!.Services.GetRequiredService<IRouteDisplayViewModel>();
    public IRouteEnginesViewModel RouteEnginesViewModel => Host!.Services.GetRequiredService<IRouteEnginesViewModel>();

    protected override void SetupConfigurationEnvironment( IConfigurationBuilder builder )
    {
        base.SetupConfigurationEnvironment( builder );

        builder.SetBasePath( Environment.CurrentDirectory )
            .AddJsonFile( Path.Combine( ApplicationConfigurationFolder, AppConfigFile ), false,false )
            .AddJsonFile( Path.Combine( UserConfigurationFolder, UserConfigFile ), true, false )
            .AddUserSecrets<CompositionRoot>();
    }

    protected override void RegisterViewModels( ViewModelDependencyBuilder builder )
    {
        base.RegisterViewModels( builder );

        builder.RegisterViewModelInterface<IMainViewModel>()
            .DesignTime<DesignTimeMainViewModel>()
            .RunTime<MainViewModel>();

        builder.RegisterViewModelInterface<IOptionsViewModel>()
            .DesignTime<DesignTimeOptionsViewModel>()
            .RunTime<OptionsViewModel>();

        builder.RegisterViewModelInterface<IRouteDisplayViewModel>()
            .DesignTime<DesignTimeRouteDisplayViewModel>()
            .RunTime<RouteDisplayViewModel>();

        builder.RegisterViewModelInterface<IRouteEnginesViewModel>()
            .DesignTime<DesignTimeRouteEnginesViewModel>()
            .RunTime<RouteEnginesViewModel>();

        builder.RegisterViewModelInterface<IProcessorViewModel>()
            .DesignTime<DesignTimeProcessorViewModel>()
            .RunTime<ProcessorViewModel>();
    }
}
```

In the `RegisterViewModels()` override you use the provided `ViewModelDependencyBuilder`
to register classes for particular interfaces to be used at either run-time or
design-time. The library ensures the correct one is registered with `Autofac` when
the dependency injection system initializes.

If you don't specify a separate design-time class the run-time class is used by
default.

[table of contents](#Table-of-Contents)

#### J4JViewModelLocator Usage
You typically register an instance of `J4JViewModelLocator` in your WPF app's
`App.xaml` file:
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
                    <local:CompositionRoot x:Key="ViewModelLocator" />
                    <local:TextToDouble x:Key="textToDouble" />
                </ResourceDictionary>

                <ResourceDictionary Source="CustomComboBox.xaml"/>

            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>

    </Application.Resources>
</Application>
```

The particular structure used in the example to define the `ResourceDictionary`
was needed because there are a bunch of other merged dictionaries in the actual
code base the example is pulled from. I'm not showing the rest of that stuff here.

Note that **Startup** and **Exit** methods are different from what you normally
find in a default WPF application. That's because the `App.xaml.cs` code-behind 
needs to look like this to provide dependency injection support to the WPF
framework:

```csharp
public partial class App : Application
{
    private async void Application_Startup( object sender, StartupEventArgs e )
    {
        var compRoot = TryFindResource( "ViewModelLocator" ) as CompositionRoot;
        if( compRoot?.Host == null )
            throw new NullReferenceException( "Couldn't find ViewModelLocator resource" );

        await compRoot.Host.StartAsync();

        var mainWindow = compRoot.Host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private async void Application_Exit( object sender, ExitEventArgs e )
    {
        var compRoot = (CompositionRoot) TryFindResource( "ViewModelLocator" );

        using( compRoot.Host! )
        {
            await compRoot.Host!.StopAsync();
        }
    }
}
```

For a complete example of how `J4JViewModelLocator` can be used check out my
[GeoProcessor](https://github.com/markolbert/GeoProcessor) repository and look 
through the [GeoProcessorWPF](https://github.com/markolbert/GeoProcessor/tree/master/GeoProcessorWPF) 
project.

[table of contents](#Table-of-Contents)

#### Color Conversion
There are two different **Color** objects inside of the C# libraries, 
*System.Drawing.Color* and *System.Windows.Media.Color*. The 
static class `MediaDrawing` provides methods for converting from 
`System.Windows.Media.Color` to `System.Drawing.Color`.

[table of contents](#Table-of-Contents)
