# J4JSoftware.J4JCommandLine: Usage

- [Creating the Builder Configuration](#creating-the-builder-configuration)
- [Building IHost](#building-ij4jhost)
- [Configuring J4JHostConfiguration](#configuring-j4jhostconfiguration)

You create a `IJ4JHost` instance by configuring an instance of `J4JHostConfiguration` and calling its `Build()` method.

## Creating the Builder Configuration

`J4JHostConfiguration` has a single public constructor taking two parameters, one of which has a default value:

```csharp
    public J4JHostConfiguration(
        AppEnvironment appEnvironment,
        bool registerJ4JHost = true
    )
    {...}
```

|Parameter|Type|Description|
|---------|:--:|-----------|
|appEnvironment|`AppEnvironment`|the environment in which the code is running|
|registerJ4JHost|`bool`|a flag indicating whether or not the created `IJ4JHost` instance should itself be registered with the dependency injection system, and thereby be available through the `IServiceProvider` interface|

`AppEnvironment` is an enum which can take on one of the following values:

```csharp
public enum AppEnvironment
{
    Console,
    WpfDesignMode,
    Wpf,
    UnpackagedWinApp,
    PackagedWinApp
}
```

`UnpackagedWinApp` is currently not utilized and exists for potential future use.

`AppEnvironment` affects where the API looks for application configuration files. This applies in the case of WPF apps, which run in both "runtime mode" and "design mode" (within the Visual Studio context).

## Building IJ4JHost

Building an instance of `IJ4JHost` can be very simple:

```csharp
var hostConfig = new J4JHostConfiguraton();
// configuration steps omitted

var host = hostConfig.Build();
```

However, it's recommended to check to ensure the `J4JHostConfiguration` object is properly configured before calling `Build()`:

```csharp
var hostConfig = new J4JHostConfiguraton();
// configuration steps omitted

IJ4JHost? host = null;

if( hostConfig.MissingRequirements == J4JHostRequirements.AllMet )
    host = hostConfig.Build();
else 
{
    // take remedial action, abort startup, etc.
}
```

`J4JHostRequirements` is a flagged enum which can contain one or more of the following flags:

```csharp
[ Flags ]
public enum J4JHostRequirements
{
    Publisher = 1 << 0,
    ApplicationName = 1 << 1,
    AvailableTokens = 1 << 2,
    OptionsGenerator = 1 << 3,

    AllMet = 0
}
```

The first two flags indicate you failed to define the publisher name, the application name, or both. These are necessary for the data protection system to be configured, and for the configuration file folders to be located in some situations.

The third flag indicates you configured the command line processing subsystem to use specific lexical elements (e.g., the text that demarcates a string value) but failed to provide them.

The fourth flag indicates you configured the command line processing subsystem but failed to define any command line options.

Even if `J4JHostConfiguration` is properly configured, and an `IJ4JHost` instance is returned by `Build()`, it's still possible problems were encountered during the build process. These are logged to `J4JHostConfiguration.Logger`, and can be output to the runtime logger (assuming you configured one) via `J4JHostConfiguration.OutputBuildLogger()`.

## Configuring J4JHostConfiguration

### Introduction

You configure an instance of `J4JHostConfiguration` by calling various extension methods. I've broken these out into groups to explain them.

Note that there are a few extension methods you **must** call. These are outlined under **Basic Configuration**.

Click the links for more details.

- [Basic Configuration](#basic-configuration)
- [Configuration Files](#adding-configuration-files)
- [J4JCommandLine Subsystem](j4jcmdline.md)
- [Dependency Injection Configuration](#dependency-injection)
- [J4JLogging Subsystem](#configuring-logging)
- [Adding Services](#adding-services)
- [Other J4JHostConfiguration Extension Methods](#other-j4jhostconfiguration-extension-methods)
- [Helper Methods](#helper-methods)

### Basic Configuration

There are two properties which **must** be defined for `J4JHostConfiguration` to be buildable. You can also explicitly set the file system case sensitivity, which affects how the command line processor works, if the automatic detection based on `Environment.OSVersion.Platform` isn't sufficient.

|Purpose|Required / Optional|J4JHostConfiguration Extension Method|
|-------|:-----------------:|----------------|
|Set publisher's name|**required**|Publisher( this J4JHostConfiguration config, string publisher )|
|Set application name|**required**|ApplicationName( this J4JHostConfiguration config, string name )|
|Set file system case sensitivity|*optional*, defaults based on the detected operating system|FileSystemCaseSensitivity( this J4JHostConfiguration config, bool caseSensitivity )|

### Adding Configuration Files

`J4JHostConfiguration` includes the concept of user and application configuration files. Conceptually, application configuration files are intended to define run-time behaviors which are independent of any particular user, or apply to all users. User configuration files store information which customizes and application's behavior for a specific user.

While the `IConfiguration` system supports adding various kinds of configuration files, `J4JHostConfiguration` **requires that all configuration files be JSON formatted**.

There are two extension methods for adding configuration files to `J4JHostConfiguration`, one for application configuration files and one for user configuration files:

```csharp
public static J4JHostConfiguration AddApplicationConfigurationFile( 
    this J4JHostConfiguration config,
    string filePath,
    bool optional = true,
    bool reloadOnChange = false
    )

public static J4JHostConfiguration AddUserConfigurationFile(
    this J4JHostConfiguration config,
    string filePath,
    bool optional = true,
    bool reloadOnChange = false
        )
```

You can indicate whether a file is required or optional (the default is optional), and whether or not the configuration system should watch the file and reload it if it changes.

You can add as many of each kind of file as you like.

*Note that the `J4JCommandLine` system allows you to specify application files on the command line. If you take advantage of that functionality, any application configuration files you explicitly add via `AddApplicationConfigurationFile` are ignored*.

#### Searching for Application Configuration Files

### Configuring Command Line Processing

`J4JHostConfiguration` uses my `J4JCommandLine` library to support configuration via the command line. `J4JCommandLine` is designed to integrate with the `IConfiguration` system. You can learn more about its capabilities from its [GitHub documentation](https://github.com/markolbert/J4JCommandLine).

Including command line processing in `J4JHostConfiguration` starts with calling the `AddCommandLineProcessing` extension method:

```csharp
public static J4JCommandLineConfiguration AddCommandLineProcessing( this J4JHostConfiguration config,
        CommandLineOperatingSystems? operatingSystem = null )
```

The optional `CommandLineOperatingSystems` value lets you override the default selection, which is based on `Environment.OSVersion.Platform`. All operating systems from Microsoft, including XBox, are assumed to use Windows-style command line flag delimiters (e.g., '/'). Macintosh and Unix operating systems are assumed to use Linux-style command line flag delimiters (e.g., '-'). Any other operating system uses Linux-style flag delimiters (there aren't any other operating systems defined as of late 2022).

Note that `AddCommandLineProcessing` returns a `J4JCommandLineConfiguration` object rather than a `J4JHostConfiguration` object. That's because there are quite a number of configuration options for `J4JCommandLine`, so including them in `J4JHostConfiguration` directly would make the codebase more complicated than I wanted it to be.

For more information about configuring command line options, please see [this documentation](j4jcmdline.md).

I generally all the `AddCommandLineProcessing` extension method last when configuring `J4JHostConfiguration` so as not to break the "declarative chain" for `J4JHostConfiguration`.

### Dependency Injection

A key reason for using `IHost`/`IJ4JHost` is to expose your dependency injection system through Microsoft's `IServiceProvider` system. `J4JHostConfiguration`, which uses `Autofac`, takes care of calling the appropriate methods to do this. You only need to be concerned with registering the types you want to be available through dependency injection.

That's done by calling the `AddDependencyInjectionInitializers` extension method, supplying methods you define which accept two parameters, a `HostBuilderContext` instance and an `Autofac` `ContainerBuilder` instance:

```csharp
public static J4JHostConfiguration AddDependencyInjectionInitializers( this J4JHostConfiguration config,
    params Action<HostBuilderContext,
        ContainerBuilder>[] initializers )
```

You can supply as many dependency injection initializer methods as you want, although in most cases I find I only use one.

### Configuring Logging

`IJ4JHost` uses my [J4JLogging library](https://github.com/markolbert/J4JLogging), which is itself based on my favorite logging library, [Serilog](https://serilog.net/). Including logging is optional, and you configure it using several extension methods.

You configure your logging options by supplying a configuration method to `J4JHostConfiguration` through the `LoggerInitializer` extension method:

```csharp
public static J4JHostConfiguration LoggerInitializer( this J4JHostConfiguration config, 
Action<IConfiguration, J4JHostConfiguration, J4JLoggerConfiguration> initializer )
```

The initializer method receives instances of `IConfiguration`, `J4JHostConfiguration` and `J4JLoggerConfiguration`. Details on how to configure `IJ4JLogger` can be found in the [library's GitHub documentation](https://github.com/markolbert/J4JLogging).

Further details about configuring logging can be [found here](j4jlogging.md).

### Adding Services

You can add services to `IJ4JHost` by calling the `AddServicesInitializer` extension method:

```csharp
public static J4JHostConfiguration AddServicesInitializers(
    this J4JHostConfiguration config,
    params Action<HostBuilderContext, IServiceCollection>[] initializers)
```

Any service you add through the dependency injection initializer is automatically registered as an `IJ4JHost` service, so you don't need to add them separately (in practice, I hardly ever add services explicitly for this very reason).s

### Other J4JHostConfiguration Extension Methods

There are a few lesser-used (at least by me) extension methods available for configuring `J4JHostConfiguration`.

You can specify `Action<HostBuilderContext, IConfigurationBuilder>` environment initializer methods by calling the `AddEnvironmentInitializers` extension method:

```csharp
public static J4JHostConfiguration AddEnvironmentInitializers( 
    this J4JHostConfiguration config,
    params Action<HostBuilderContext, IConfigurationBuilder>[] initializers )
```

The added actions control or modify the entire build process (i.e., I believe these methods get called very early in the build process).

You can also add additional `IConfiguration` components to the configuration environment by calling `AddConfigurationInitializers`:

```csharp
public static J4JHostConfiguration AddConfigurationInitializers(
    this J4JHostConfiguration config,
    params Action<IConfigurationBuilder>[] initializers)
```

You would use this, for example, to add user secrets as a configuration source, or a non-JSON configuration file.

### Helper Methods

There is also a helper method which may be useful for logging. It converts an instance of `J4JHostRequirements` into a formatted text string:

```csharp
public static string ToText( this J4JHostRequirements requirements )
```
