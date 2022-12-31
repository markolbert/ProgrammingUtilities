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

Note that there are a few extension methods you **must** call. These are outlined under **Basic Configuration**.

Click the links for more details.

- [Basic Configuration](#basic-configuration)
- [Dependency Injection Configuration](#dependency-injection)
- [J4JLogging Subsystem](#configuring-logging)
  - [Basic Configuration](#basic-configuration)
  - [Adding a NetEventSink](#adding-a-neteventsink)
  - [Trimming Source Code File Paths](#trimming-source-code-file-paths)
- [IConfiguration Subsystem](iconfiguration.md)
- [J4JCommandLine Subsystem](j4jcmdline.md)

## Basic Configuration

There are two properties which **must** be defined for `J4JHostConfiguration` to be buildable. You can also explicitly set the file system case sensitivity, which affects how the command line processor works, if the automatic detection based on `Environment.OSVersion.Platform` isn't sufficient.

|Purpose|Required / Optional|J4JHostConfiguration Extension Method|
|-------|:-----------------:|----------------|
|Set publisher's name|**required**|Publisher( this J4JHostConfiguration config, string publisher )|
|Set application name|**required**|ApplicationName( this J4JHostConfiguration config, string name )|
|Set file system case sensitivity|*optional*, defaults based on the detected operating system|FileSystemCaseSensitivity( this J4JHostConfiguration config, bool caseSensitivity )|

### Dependency Injection

A key reason for using `IHost`/`IJ4JHost` is to expose your dependency injection system through Microsoft's `IServiceProvider` system. `J4JHostConfiguration`, which uses `Autofac`, takes care of calling the appropriate methods to do this. You only need to be concerned with registering the types you want to be available through dependency injection.

That's done by calling the `AddDependencyInjectionInitializers` extension method, supplying methods you define which accept two parameters, a `HostBuilderContext` instance and an `Autofac` `ContainerBuilder` instance:

```csharp
public static J4JHostConfiguration AddDependencyInjectionInitializers( this J4JHostConfiguration config,
    params Action<HostBuilderContext,
        ContainerBuilder>[] initializers )
```

You can supply as many dependency injection initializer methods as you want, although in most cases I find I only use one.

## Configuring Logging

### Basic J4JLogging Configuration

`IJ4JHost` uses my [J4JLogging library](https://github.com/markolbert/J4JLogging), which is itself based on my favorite logging library, [Serilog](https://serilog.net/). Including logging is optional, and you configure it using several extension methods.

You configure your logging options by supplying a configuration method to `J4JHostConfiguration` through the `LoggerInitializer` extension method:

```csharp
public static J4JHostConfiguration LoggerInitializer( this J4JHostConfiguration config, 
Action<IConfiguration, J4JHostConfiguration, J4JLoggerConfiguration> initializer )
```

The initializer method receives instances of `IConfiguration`, `J4JHostConfiguration` and `J4JLoggerConfiguration`. Details on how to configure `IJ4JLogger` can be found in the [library's GitHub documentation](https://github.com/markolbert/J4JLogging).

### Adding a NetEventSink

In my Windows desktop apps, I often want to make some or all of the app's log messages available in the UI. `Serilog` doesn't directly support this, so far as I know, so I built it into `J4JLogger` as a custom `Serilog` sink called `NetEventSink`. You can add such a sink to an `IJ4JHost` instance by calling an extension method on `J4JHostConfiguration`:

```csharp
public static J4JHostConfiguration AddNetEventSinkToLogger(
    this J4JHostConfiguration config,
    string? outputTemplate = null,
    LogEventLevel minimumLevel = LogEventLevel.Verbose
)
```

`NetEventSink` allows you to filter what log events get sent to the UI sink based on `LogEventLevel`. It also allows you to define a different `Serilog` template for events sent to the UI sink. These are specified through the optional `outputTemplate` and `minimumLevel` method arguments.

### Trimming Source Code File Paths

One of the main additions to `Serilog` in `IJ4JLogger` (which was, in fact, the reason I wrote it) involves automatically adding source code annotations (e.g., source code file path, line number) to log events. Unfortunately, source code file paths typically get very long very quickly, so I implemented a way to shorten them.

It's done by supplying a method to trim file paths to `J4JHostConfiguration`. That's done by calling the `FilePathTrimmer` extension method:

```csharp
public static J4JHostConfiguration FilePathTrimmer(
    this J4JHostConfiguration config, 
    Func<Type?, string, int, string, string> filePathTrimmer )
```

The `Func<>` signature shows the parameters passed to the trimming function. There's a reference implementation contained in the `J4JLogging` library (it's defined in the static class `SourceCodeFilePathModifiers`) which I typically copy to each of my projects, and then supply to `J4JHostConfiguration`. It consists of two relatively simple methods, the first of which is the one supplied to `J4JHostConfiguration.FilePathTrimmer`:

```csharp
// copy these next two methods to the source code file where you configure J4JLogger
// and then reference FilePathTrimmer as the context converter you
// want to use
private static string FilePathTrimmer( Type? loggedType,
    string callerName,
    int lineNum,
    string srcFilePath )
{
    return CallingContextEnricher.DefaultFilePathTrimmer( loggedType,
                                                            callerName,
                                                            lineNum,
                                                            CallingContextEnricher.RemoveProjectPath( srcFilePath,
                                                                GetProjectPath() ) );
}

private static string GetProjectPath( [ CallerFilePath ] string filePath = "" )
{
    // DirectoryInfo will throw an exception when this method is called on a machine
    // other than the development machine, so just return an empty string in that case
    try
    {
        var dirInfo = new DirectoryInfo(System.IO.Path.GetDirectoryName(filePath)!);

        while (dirInfo.Parent != null)
        {
            if (dirInfo.EnumerateFiles("*.csproj").Any())
                break;

            dirInfo = dirInfo.Parent;
        }

        return dirInfo.FullName;
    }
    catch (Exception)
    {
        return string.Empty;
    }
}
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
