# J4JWinAppSupport

- [Properties](#properties)
- [Configuration](#configuration)
- [Creation and Usage](#creation-and-usage)

## Properties

`J4JWinAppSupport` exposes a number of properties which you can reference in your WinUI 3 app:

|Property|Type|Comments|
|--------|----|--------|
|`IsInitialized`|`bool`|true if the support class was initialized, false otherwise. *Improperly initialized instances mean a significant problem was encountered and the app should terminate itself*.|
|`ConfigurationFilePath`|`string`|the full path to the user's configuration file|
|`Services`|`IServiceProvider?`|the instance of `IServiceProvider` created by the `IHostBuilder`/`IHost` API, assuming initialization succeeded|
|`Protector`|`IDataProtector`|an instance of `IDataProtector`, which can be used to encrypt/decrypt strings and is required to use the application configuration file base class `AppConfigBase` and its 'automatic' encryption/decryption of sensitive strings|
|`LoggerFactory`|`ILoggerFactory?`|an instance of Microsoft's `ILoggerFactory`, assuming logging was enabled|
|`Logger`|`ILogger?`|an instance of Microsoft's `ILogger` which can be used during app startup|
|`AppConfig`|`AppConfigBase?`|an instance of the app configuration class you derived from `AppConfigBase` and which reflects the on-disk user configuration file|

[return to top](#j4jwinappsupport)

[return to overview](startup.md)

[return to readme](readme.md)

## Configuration

You customize `J4JWinAppSupport`'s behavior by creating a derived class and overriding one or more virtual methods:

|Method|Returns|Argument(s)|Comments|
|------|-------|-----------|--------|
|`GetSerilogConfiguration()`|`LoggerConfiguration?`||defines your specific `Serilog` configuration. By default it returns `null` and logging is not set up|
|`CreateHostBuilder()`|`IHostBuilder`||allows you to modify/override the default host builder configuration, which parses the user configuration file and sets up the services you want (see `ConfigureServices()` below). Generally not overridden.|
|`ConfigureServices()`|`IServiceCollection`|lets you define the services you plan on using in your app|
|||`HostBuilderContext` hbc|context information you may need to configure your services|
|||`IServiceCollection` services|the collection to which you add your defined services|

[return to top](#j4jwinappsupport)

[return to overview](startup.md)

[return to readme](readme.md)

## Creation and Usage

Creating an instance of `J4JWinAppSupport` requires only one parameter, the name (*not the path*) of the file holding a user's configuration information:

```csharp
protected J4JWinAppSupport(
    string configFileName = "userConfig.json"
    )
```

You use the class you derive from `J4JWinAppSupport` like this:

```csharp
public partial class App
{
    internal new static App Current => (App) Application.Current;

    public App()
    {
        this.InitializeComponent();

        if (!AppSupport.Initialize())
            Exit();

        Services = AppSupport.Services!;
    }

    internal WinAppSupport AppSupport { get; } = new WinAppSupport();
    internal IServiceProvider Services { get; }
}
```

Key details here involve overriding the definition of `Current` so it points to an instance of your app's `App` class, and the inclusion of the `AppSupport` and `Services` properties. The latter lets you use the retrieve services from within your app's classes in a way which lets you get the benefits of dependency injection even though WinUI 3 doesn't directly support constructor injection.

[return to top](#j4jwinappsupport)

[return to overview](startup.md)

[return to readme](readme.md)
