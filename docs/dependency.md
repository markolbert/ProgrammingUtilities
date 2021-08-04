# Dependency Injection

Provides a general purpose composition root, which also sets up
`IJ4JLogger` and provides data encryption/decryption capabilities via 
`Microsoft.AspNetCore.DataProtection`. The dependency injection
capabilities are provided by [Autofac](https://autofac.org/).

This assembly targets Net5 and has nullability enabled.

## <a name='TableofContents'></a>Table of Contents
<!-- vscode-markdown-toc -->
* [Changes](#Changes)
* [Initialization](#Initialization)
  * [Initialization: Xaml Apps](#Initialization:XamlApps)
* [Constructor Parameters](#ConstructorParameters)
  * [Constructor Parameters: Console Apps](#ConstructorParameters:ConsoleApps)
  * [Constructor Parameters: Xaml Apps](#ConstructorParameters:XamlApps)
* [J4JLogger Configuration](#J4JLoggerConfiguration)
* [Configuring IHost Capabilities](#ConfiguringIHostCapabilities)
* [Logging During Setup](#LoggingDuringSetup)
* [Data Protection Services](#DataProtectionServices)

<!-- vscode-markdown-toc-config
	numbering=false
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

## <a name='Changes'></a>Changes

|Version|Summary of Changes|
|-------|------------------|
|2.0|Added support for XAML-based projects (e.g., WPF). Added support for configuring `J4JLogger` from `IConfiguration` API.|

## <a name='Initialization'></a>Initialization

I'm a huge fan of [Autofac](https://autofac.org/) and use it in just about everything I write.
But I found there was a lot of boilerplate code I had to use to integrate it with the Net5 
`IConfiguration` system, my own `IJ4JLogger` functionality, etc. This assembly is my way of 
simplifying that setup.

The key class, `CompositionRootBase` is essentially a wrapper around the Net5 `IHostBuilder` 
and `IHost` systems. You don't derive from `CompositionRootBase` directly. Instead, you use
one of its specialized derivations, `ConsoleCompositionRoot` or `XAMLCompositionRoot`. The former
targets command-line apps while the latter targets XAML-based apps (e.g., WPF).

In either case you start by creating a derived class in your app:

```csharp
public class CompositionRoot : ConsoleCompositionRoot
{
    private static CompositionRoot? _compRoot;

    public static CompositionRoot Default
    {
        get
        {
            if( _compRoot != null ) 
                return _compRoot;

            _compRoot = new CompositionRoot();
            _compRoot.Build();

            return _compRoot;
        }
    }

    private CompositionRoot()
        : base(
            "J4JSoftware",
            Program.AppName,
            loggingConfigType: typeof(AppConfig)
        )
    {
    }
}
```

**It is critical you remember to call `Build()` after creating a new instance of your composition root**.
The composition root classes cannot provide any services unless you do because the service provider
will be undefined.

I typically use this *static property/private constructor* pattern because I want to have only 
one instance of my composition root throughout my app (it *is* a root, after all :)).

[return to Table of Contents](#TableOfContents)

## <a name='Initialization:XamlApps'></a>Initialization: Xaml Apps

The initialization pattern for XAML apps is slightly different, at least for WPF apps. That's because
in WPF apps you typically implement the composition root pattern by creating a StaticResource in your
App.xaml file:

```xml
<ResourceDictionary>
    <local:CompositionRoot x:Key="CompositionRoot" />
    <local:TextToDouble x:Key="textToDouble" />
</ResourceDictionary>
```

Resource objects must have a *parameterless public constructor* which *fully configures* the instance
that's being created. Consequently, you can't call `Build()` on it after it's created.

What you can do is include the call to `Build()` in your composition root's constructor:

```csharp
public sealed class CompositionRoot : XamlCompositionRoot
{
    public const string AppName = "GeoProcessor";
    public const string AppConfigFile = "appConfig.json";
    public const string UserConfigFile = "userConfig.json";

    private static CompositionRoot? _compRoot;

    public static CompositionRoot Default
    {
        get
        {
            _compRoot ??= new CompositionRoot();
            return _compRoot;
        }
    }

    public CompositionRoot()
        : base(
            "J4JSoftware",
            AppName,
            () => DesignerProperties.GetIsInDesignMode( new DependencyObject() ),
            "J4JSoftware.GeoProcessor.DataProtection", 
            loggingConfigType:typeof(AppConfig)
        )
    {
        Build();
    }
}
```

This requires you seal your composition root class to avoid pesky warning messages. But
that shouldn't be a problem because you wouldn't want to derive from it. For details on the
other different features of the `XamlCompositionRoot` class please see xxx.

[return to Table of Contents](#TableOfContents)

## <a name='ConstructorParameters'></a>Constructor Parameters

The `CompositionRootBase` constructor requires several parameters and supports several
optional parameters:

```csharp
protected CompositionRootBase(
    string publisher,
    string appName,
    string? dataProtectionPurpose = null,
    Type? loggingConfigType = null,
    ILoggerConfigurator? loggerConfigurator = null,
    params Assembly[] loggerChannelAssemblies
    )
```

|Parameter|Type|Required?|Purpose|
|---------|----|---------|-------|
|publisher|string|Yes|identifies the publisher so that user configuration folders can be organized with one folder per publisher|
|appName|string|Yes|identifies the app so that user configuration folders can be organized with one folder per app inside the publisher's folder|
|dataProtectionPurpose|string?|Optional|the key for data protection services provided; defaults to the type name if not supplied|
|loggingConfigType|Type?|Optional|the Type holding information needed to configure the J4JLogger service; if not supplied no configuration information will be available (which may not be necessary in certain simple situations, e.g., unit testing)|
|loggerConfigurator|ILoggerConfigurator?|Optional|a J4JLogger configuration helper; if not supplied, defaults to `LoggerConfigurator`|
|loggerChannelAssemblies|params Assembly[]|Optional|assemblies to search for `J4JLogger` channels; **J4JSoftware.J4JLogger** is always included and searched|

The publisher and app name jointly define (again, on Windows at least) the
user's local settings folder. On Windows this is generally located at 
C:\\Users\\**[user name]**\\AppData\\Local\\**[publisher]**\\**[appName]**\\.

[return to Table of Contents](#TableOfContents)

## <a name='ConstructorParameters:ConsoleApps'></a>Constructor Parameters: Console Apps

The `ConsoleCompositionRoot` constructor adds one more parameter to the `CompositionRootBase`
parameters:

|Parameter|Type|Required?|Purpose|
|---------|----|---------|-------|
|useConsoleLifetime|bool|Yes, but defaults to **true**|controls whether or not the console's lifetime defines the host's lifetime (I think; I always leave it as the default)|

## <a name='ConstructorParameters:XamlApps'></a>Constructor Parameters: Xaml Apps

The `XamlCompositionRoot` constructor adds one more parameter to the `CompositionRootBase`
parameters:

|Parameter|Type|Required?|Purpose|
|---------|----|---------|-------|
|inDesignMode|Func&lt;bool&gt;|Yes|a function reference used to determine whether or not the instance is running in design mode|

Knowing whether the app is running in design mode or run time mode is important because the execution folder is different in design mode, at least under WPF (which is where I've been doing most of my UI desktop UI programming). That affects the location of things like application configuration files that Visual Studio copies to the execution directory. `XamlCompositionRoot` makes the required adjustments internally so you don't have to.

In a WPF app this is typically handled like this:

```csharp
public CompositionRoot()
    : base(
        "J4JSoftware",
        AppName,
        () => DesignerProperties.GetIsInDesignMode( new DependencyObject() ),
        "J4JSoftware.GeoProcessor.DataProtection", 
        loggingConfigType:typeof(AppConfig)
    )
{
    Build();
}
```

Note also the call to `Build()` in the body of the constructor, and the fact that the constructor
is **public** not **private** like it is in examples using `ConsoleCompositionRoot`.

That's because to serve as a StaticResource the composition root must be have a public parameterless constructor. Since you must build the object the build call must be in the constructor. That also requires the class to be sealed since `Build()` is a protected virtual method.

[return to Table of Contents](#TableOfContents)

## <a name='J4JLoggerConfiguration'></a>J4JLogger Configuration

The `J4JLogger` system needs to be configured to function. While there are various ways of doing that (see [J4JLogger](https://github.com/markolbert/J4JLogging) for details) it is convenient, since Microsoft's `IHost` API incorporates the `IConfiguration` system, to do so from within my composition root API.

`CompositionRootBase` makes no assumptions about how you want to structure the configuration information for `J4JLogger`. Consequently, out of the box my composition root system will provide a useless `J4JLogger`: you'll be able to write to it, but you'll never see any output.

To get it to work you have to pass additional information thru the composition root constructors. Depending upon how well the defaults I've defined work for you that may be as simple as passing in the Type of your configuration object. The value of `loggingConfigType` in the following example means the logging configuration information is contained in an instance of `AppConfig`:

```csharp
private CompositionRoot()
    : base(
        "J4JSoftware",
        Program.AppName,
        dataProtectionPurpose: "J4JSoftware.GeoProcessor.DataProtection",
        loggingConfigType:typeof(AppConfig)
    )
{
}
```

There's one important assumption about specifying `AppConfig`: *you must register a way to create an instance of it with the dependency injection framework* that's part of `CompositionRootBase` and its derivatives. If you don't an exception will be thrown at startup. I typically do that like this:

```csharp
// this is an override in your app's composition root class
protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
{
    base.SetupDependencyInjection( hbc, builder );

    builder.Register( c =>
        {
            var retVal = hbc.Configuration.Get<AppConfig>();

            // you can ignore these next two lines for purposes of this example
            retVal.ApplicationConfigurationFolder = ApplicationConfigurationFolder;
            retVal.UserConfigurationFolder = UserConfigurationFolder;

            return retVal;
        } )
        .AsSelf()
        .AsImplementedInterfaces()
        .SingleInstance();
}
```
This will work, as is...**provided** the configuration Type reference you passed to the constructor contains a property holding an instance of `LoggerInfo`, a simple configuration class defined in the `J4JLogger` library:

```csharp
public class LoggerInfo
{
    public ChannelConfiguration? Global { get; set; }
    public List<string>? Channels { get; set; }
    public Dictionary<string, ChannelConfiguration>? ChannelSpecific { get; set; }

    public IEnumerable<string> AllChannels( params string[] channels )
    {
        if( Channels == null && ChannelSpecific == null && channels.Length == 0 )
            yield break;

        var allChannels = new List<string>();

        if( Channels != null )
            allChannels.AddRange( Channels );

        allChannels.AddRange( channels );

        if( ChannelSpecific != null )
            allChannels.AddRange( ChannelSpecific.Select( kvp => kvp.Key ) );

        foreach( var channel in allChannels.Distinct( StringComparer.OrdinalIgnoreCase ) )
        {
            yield return channel.ToLower();
        }
    }
}
```

For the `IConfiguration` system to be able to create an instance of `LoggerInfo` with the parameters we passed to the composition root's constructor you must format the logger configuration information in a way compatiable with `LoggerInfo`. For a JSON file that might look like this:

```json
  "Logging": {
    "Global": {
      "SourceRootPath": "C:\\Programming\\GeoProcessor\\",
      "IncludeSourcePath":  true 
    },
    "ChannelSpecific": {
      "Debug": {
        "MinimumLevel": "Information"
      },
      "Console": {
        "MinimumLevel": "Information",
        "RequireNewLine":  true 
      }
    }
  }
```

There are other configuration options which `LoggerInfo` understands. You can read about them at [J4JLogger](https://github.com/markolbert/J4JLogging).

The last step in wiring up the logger configuration is to override 

It can be provided two ways:

- by calling `StaticConfiguredLogging()` with an instance of
`IJ4JLoggerConfiguration`. This approach ignores any logger configuration
information contained in the Net5 `IConfiguration` system.
- by calling `ConfigurationBasedLogging()` with an instance of 
`IChannelConfigProvider`. This utililzes the `IConfiguration` system
information to configure the logger.

For information on configuring `IJ4JLogger` see the 
[github documentation](https://github.com/markolbert/J4JLogging).

[table of contents](#Table-of-Contents)

## <a name='ConfiguringIHostCapabilities'></a>Configuring IHost Capabilities

At this point your composition root won't work because you haven't told
it how to load any configuration information, and it needs at least the
logging configuration information. It also wouldn't be able to act as
a useful composition root because you haven't registered anything with
the dependency injection system.

All of the functionality of `IHostBuilder` is available for use during
setup. You access it by overriding various protected methods:

|Virtual Protected Method|Corresponding IHostBuilder Method|What It's Used For|
|------------------------|-------------------------|--------------------------------------|
|SetupAppEnvironment|ConfigureAppConfiguration|set up the overall `IHostBuilder` environment|
|SetupConfigurationEnvironment|ConfigureHostConfiguration|define your `IConfigurationBuilder`|
|SetupDependencyInjection|ConfigureContainer\<ContainerBuilder\>|set up Autofac dependency injection|
|SetupServices|ConfigureServices|configure services|

Some of these virtual methods could be replaced by simply calling the
corresponding `IHostBuilder` method directly (`J4JCompositionRoot<>`
exposes it as a protected property).

But that would break the `IJ4JLogger` and data protection featurs so I
don't recommend it. Besides, I personally find it more intuitive to
override a protected virtual method than I do to use lambda expressions for
complicated setups. And registering types for dependency injection
*always* involves a bunch of code :).

## <a name='LoggingDuringSetup'></a>Logging During Setup

Another nice feature of `J4JCompositionRoot<>` is that you can log events
and problems. This normally can't be done because the logging system
(at least `IJ4JLogger` and the underlying [`Serilog`](https://serilog.net/)
logger it uses) isn't set up yet (i.e., it's a chicken and egg problem).

`J4JCompositionRoot<>` avoids this by exposing a protected instance 
of `J4JCachedLogger` which you can use in any of the method overrides
you define:

```csharp
protected override void SetupConfigurationEnvironment( IConfigurationBuilder builder )
{
    base.SetupConfigurationEnvironment( builder );

    var options = new OptionCollection( CommandLineStyle.Linux, loggerFactory: () => CachedLogger );
    
    // abbreviated
}

protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
{
    base.SetupDependencyInjection( hbc, builder );

    builder.Register(c =>
    {
        AppConfig? config = null;

        try
        {
            config = hbc.Configuration.Get<AppConfig>();
        }
        catch (Exception e)
        {
            CachedLogger.Fatal<string>(
                "Failed to parse configuration information. Message was: {0}",
                e.Message);
        }

        // abbreviated
    }
}
```

In the first code block `CachedLogger` is passed (admittedly as a factory
method) to the `OptionCollection` constructor so problems encountered
by the command line parser can be capture.

The second code block is an example of recording a fatal problem (which
is dealt with later in the application code).

## <a name='DataProtectionServices'></a>Data Protection Services

`J4JCompositionRoot<>` provides encryption and decryption functionality
via the `Microsoft.AspNetCore.DataProtection` system. You access it
with two methods:

|Method|Purpose|Arguments|
|-------------|-------------|-------------|
|Protect|encrypts text|**plainText** (string): the text to encrypt<br>**encrypted** (out string?): the encrypted text, if the encryption succeeded|
|Unprotect|decrypts text|**encryptedText** (string): the text to decrypt<br>**decrypted** (out string?): the decrypted text, if the decryption succeeded|

Both methods return true if they succeed or false if they fail.
