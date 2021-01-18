## Dependency Injection
Provides a general purpose composition root, which also sets up
`IJ4JLogger` and provides data encryption/decryption capabilities via 
`Microsoft.AspNetCore.DataProtection`. The dependency injection
capabilities are provided by [Autofac](https://autofac.org/).

This assembly targets Net5 and has nullability enabled.

#### Table of Contents
- [Initialization](#initialization)
- [IJ4JLogger Configuration](#IJ4JLogger-Configuration)
- [Configuring IHost Capabilities](#Configuring-IHost-Capabilities)
- [Logging During Setup](#Logging-During-Setup)
- [Data Protection Services](#Data-Protection-Services)

#### Initialization
I'm a huge fan of [Autofac](https://autofac.org/) and use it in just about everything I write.
But I found there was a lot of boilerplate code I had to use to 
integrate it with the Net5 `IConfiguration` system, my own `IJ4JLogger`
functionality, etc. This assembly is my way of simplifying that setup.

The key class, `J4JCompositionRoot<>` is essentially a wrapper around the
Net5 `IHostBuilder` and `IHost` systems. You use it by creating a
derived class, specifying the type your app uses to configure the
`IJ4JLogger` system (see the github documentation for 
[`IJ4JLogger`](https://github.com/markolbert/J4JLogging) for
more details):
```csharp
public class CompositionRoot : J4JCompositionRoot<J4JLoggerConfiguration>
{
    public static CompositionRoot Default { get; }

    static CompositionRoot()
    {
        Default = new CompositionRoot()
        {
            LoggingSectionKey = "Logging",
            UseConsoleLifetime = true
        };

        Default.ChannelInformation
            .AddChannel<ConsoleConfig>( "Logging:Channels:Console" )
            .AddChannel<DebugConfig>( "Logging:Channels:Debug" );

        Default.Initialize();
    }

    private CompositionRoot()
        : base( "J4JSoftware", Program.AppName, "J4JSoftware.GeoProcessor.DataProtection" )
    {
    }
```
I typically use this *static property/private constructor* pattern
because I want to have only one instance of my composition root throughout
my app (it *is* a root, after all :)).

You **must** call `Initialize()` on your composition root
instance for it to become functional. Behind the scenes this calls the
builder method on the (also behind the scenes) instance of `IHostBuilder`
which is used to create the instance of `IHost`.

The `J4JCompositionRoute<>` constructor requires two parameters plus one
optional one:

- **publisher**: this is the app's publisher/company/entity. It's part of
what defines (on Windows at least) the user's local settings folder.
- **appName**: the app's name, which also participates in defining the
user's local settings folder.
- **dataProtectionPurpose**: a string used to initialize the 
`IDataProtectionProvider`. If you don't specify one the type name
of your composition root class is used.

The publisher and app name jointly define (again, on Windows at least) the
user's local settings folder. On Windows this is generally located at 
C:\\Users\\**[user name]**\\AppData\\Local\\**[publisher]**\\**[appName]**\\.

[table of contents](#Table-of-Contents)

#### IJ4JLogger Configuration
The `IJ4JLogger` system needs configuration information for the logging
channels being used. This is done by specifying the
`LoggingSectionKey` and adding channel information to the protected
property `ChannelInformation` *before you call* `Initialize()`:
```csharp
        Default = new CompositionRoot()
        {
            LoggingSectionKey = "Logging",
            UseConsoleLifetime = true
        };

        Default.ChannelInformation
            .AddChannel<ConsoleConfig>( "Logging:Channels:Console" )
            .AddChannel<DebugConfig>( "Logging:Channels:Debug" );
```
You can also access the *include last event* functionality of 
`IJ4JLogger` by setting the protected property `IncludeLastEvent` (not
shown here). For information on configuring `IJ4JLogger` see the 
[github documentation](https://github.com/markolbert/J4JLogging).

[table of contents](#Table-of-Contents)

#### Configuring IHost Capabilities
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

[table of contents](#Table-of-Contents)

#### Logging During Setup
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

[table of contents](#Table-of-Contents)

#### Data Protection Services
`J4JCompositionRoot<>` provides encryption and decryption functionality
via the `Microsoft.AspNetCore.DataProtection` system. You access it
with two methods:

|Method|Purpose|Arguments|
|-------------|-------------|-------------|
|Protect|encrypts text|<ul><li>**plainText** (string): the text to encrypt</li><li>**encrypted** (out string?): the encrypted text, if the encryption succeeded</li></ul>|
|Unprotect|decrypts text|<ul><li>**encryptedText** (string): the text to decrypt</li><li>**decrypted** (out string?): the decrypted text, if the decryption succeeded</li></ul>|

Both methods return true if they succeed or false if they fail.

[table of contents](#Table-of-Contents)
