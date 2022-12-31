# J4JSoftware.J4JCommandLine: Usage

- [Creating the Builder Configuration](#creating-the-builder-configuration)
- [Building IHost](#building-ij4jhost)
- [Configuring J4JHostConfiguration](#configuring-j4jhostconfiguration)
  - [Introduction](#introduction)
  - [Dependency Injection](#dependency-injection)
  - [Adding Services](#adding-services)
  - [Build Environment Configuration and Helper Methods](#build-environment-configuration-and-helper-methods)

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

Note that there are a few extension methods you must call. These are outlined under the **Required Configuration** link.

Click the links for more details.

- [Required Configuration](required.md)
- [Dependency Injection Configuration](#dependency-injection)
- [J4JLogging Subsystem](j4jlogging.md)
- [IConfiguration Subsystem](iconfiguration.md)
- [J4JCommandLine Subsystem](j4jcmdline.md)

### Dependency Injection

In addition to the above there are some other extension methods that expose the underlying `IHostBuilder`'s functionality.

The first allows you to provide dependency injection registrations. Note that because I am a huge fan of [Autofac](https://autofac.org/) those registrations need to be based on `Autofac`.

You can add as many `Action<HostBuilderContext, ContainerBuilder>` methods as you like using the following extension method:

```csharp
public static J4JHostConfiguration AddDependencyInjectionInitializers(
    this J4JHostConfiguration config,
    params Action<HostBuilderContext, ContainerBuilder>[] initializers)
```

### Adding Services

The next extension method allows you to add services to the `IServiceCollection` that will be managed by the `IHost` instance you'll create.

Note: the `Autofac`-based dependency injection subsystem automatically includes all registrations as services available through `IHost`.

You add `Action<HostBuilderContext, IServiceCollection>` to the builder by calling this extension method:

```csharp
public static J4JHostConfiguration AddServicesInitializers(
    this J4JHostConfiguration config,
    params Action<HostBuilderContext, IServiceCollection>[] initializers)
```

### Build Environment Configuration and Helper Methods

You can specify `Action<HostBuilderContext, IConfigurationBuilder>` environment initializer methods by calling:

```csharp
public static J4JHostConfiguration AddEnvironmentInitializers( 
    this J4JHostConfiguration config,
    params Action<HostBuilderContext, IConfigurationBuilder>[] initializers )
```

The added actions control or modify the entire build process (i.e., I believe these methods get called very early in the build process).

There is currently also one helper method which may be useful for logging. It converts an instance of `J4JHostRequirements` into a formatted text string:

```csharp
public static string ToText( this J4JHostRequirements requirements )
```
