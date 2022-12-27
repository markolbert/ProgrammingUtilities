# J4JSoftware.J4JCommandLine: Usage

- [J4JSoftware.J4JCommandLine: Usage](#j4jsoftwarej4jcommandline-usage)
  - [Creating the Builder Configuration](#creating-the-builder-configuration)
  - [Building IHost](#building-ihost)
  - [Configuring J4JHostConfiguration](#configuring-j4jhostconfiguration)
    - [Introduction](#introduction)
    - [Dependency Injection](#dependency-injection)
    - [Adding Services](#adding-services)
    - [Build Environment Configuration and Helper Methods](#build-environment-configuration-and-helper-methods)

You create your customized `IHost` by configuring an instance of `J4JHostConfiguration`, calling its `CreateHostBuilder()` method and then calling the `Build()` method on the returned `IHostBuilder`.

## Creating the Builder Configuration

`J4JHostConfiguration` only has two constructors, distinguished by whether or not they accept a `Func<bool>` used to determine whether the `IHost` is being built for something running in *design mode*:

```csharp
public J4JHostConfiguration()
    : this( () => false )
{
}

public J4JHostConfiguration(
    Func<bool> inDesignMode
)
{ ... }
```

*Design mode* is significant for environments like WPF where your code may be running within a visual editor/designer. It shouldn't apply to console apps which don't offer such.

How you specify the `Func<bool>` depends on which design environment you're in. For WPF one way of doing it would be like this:

```csharp
var builder = new J4JHostConfiguration( 
    () => DesignerProperties.GetIsInDesignMode( new DependencyObject() ) 
);
```

This takes advantage of a static method which can determine if you're in design mode when you call it with a dummy `DependencyObject`. Looks odd but it works.

## Building IHost

A simplified example:

```csharp
var hostConfig = new J4JHostConfiguraton();
// configuration steps omitted

var builder = hostConfig.CreateHostBuilder();

// you'd want to check that builder is not null in real code
var host = builder.Build();
```

If everything was set up properly you'll get an `IHost` instance. If something went wrong `CreateHostBuilder()` returns `null`, after which you can check various properties of the `J4JHostConfiguration` instance:

- the `BuildStatus` property tells you at which stage `CreateHostBuilder()` failed. It can have one of the following values:
  - `NotInitialized`: something failed very early in the process (this is the initial state)
  - `NotBuilt`: one or more required configuration parameters were not set (see below)
  - `Aborted`: something in the later stages of construction failed. Currently this indicates a failure in the command line parsing subsystem, so if you haven't set up command line parsing it should not occur.
  - `Built`: an `IHostBuilder` was returned
- the `MissingRequirements` property tells you which, if any, required parameters were not set. It's a flag `Enum` which can contain one or more of the following:
  - `Publisher`: you failed to specify a publisher
  - `ApplicationName`: you failed to specify a name for the application
  - `OperatingSystem`: you failed to indicate what operating system the `IHost` is working with
- the `Logger` property may contain log events of significance emitted during the build process. At this point it's not a terribly useful feature because accessing its contents requires outputting it to an instance of `IJ4JLogger`...and if the build failed you probably won't have one :). I'm working on building a viewer to get access to the log events independently.

## Configuring J4JHostConfiguration

### Introduction

You configure an instance of `J4JHostConfiguration`  by calling various extension methods. I've broken these out into groups to explain them.

Note that there are a few extension methods you must call. These are outlined under the *Basic Setup* link.

Click the links for more details.

- [Basic Setup](basics.md)
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
