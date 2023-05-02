# J4JSoftware.DependencyInjection

|Version|Description|
|:-----:|-----------|
|2.6.0|**breaking changes**, [see details below](#260)|
|2.5.1|fixed nuget dependencies|
|2.5.0|**breaking changes**, [see details below](#250)|
|2.4.0|added new file locator API (experimental), [see details below](#240)|
|2.3.3|apply validation/searching functionality when adding application configuration files, [see details below](#233)|
|2.3.2|fix bug causing app config files to be ignored|
|2.3.1|make file system case sensitivity configurable but set by default|
|2.3.0|Updated to Net 7, updated packages, [see additional details below](#230)|
|2.2|simplified creating type tests, register IJ4JHost when creating IJ4JHostConfiguration|
|2.1|updated to Net 6|
|2.0|significant breaking changes; [see details below](#200)|

## 2.6.0

- The `IJ4JProtection` system was removed and replaced by the Microsoft `IDataProtector` system it was based on.
- Data protection via `IDataProtector` was made available during the `IJ4JHost` build process.
- Certain property names were simplified to clarify them (e.g., `BuildLogger` => `Logger`)

## 2.5.0

To make the library more generally useful logging has been migrated from [Serilog](https://serilog.net/) to Microsoft's logging
system.

In general, this means instances of `ILoggerFactory` are used as construction parameters, rather than `ILogger`.
This is because, while Serilog lets you scope an `ILogger` instance to a new type, you can only define
the scope of a Microsoft `ILogger` by calling `ILoggerFactory.CreateLogger()`.

FWIW, in my projects I continue to use Serilog behind the scenes as my logging engine. It's great!

In addition the new file locator API has been changed from 'experimental' to 'functional'.

## 2.4.0

A new/alternative file locator API was added. See [Experimental File Locator](file-locator.md) for details.

A static method for identifying the processes locking a file was added:

```csharp
public static class FileLocking
{
    public static List<Process> WhoIsLocking(string path);
}
```

The code was made available on [Slashdot](https://stackoverflow.com/questions/317071/how-do-i-find-out-which-process-is-locking-a-file-using-net) by [Waldemar Gałęzinowski](https://stackoverflow.com/users/5343480/waldemar-ga%c5%82%c4%99zinowski). I modified it to comply with Resharper feedback and my personal protocols.

## 2.3.3

The file validation API was moved into the DependencyInjection project from the MiscellaneousUtilities project because it is needed here and I didn't want to force the inclusion of experimental APIs in such a commonly-used assembly.

## 2.3.0

- Added ability to define application configuration files from the command line via `J4JCommandLineConfiguration::ConfigurationFileKeys()`.

## 2.0.0

The library takes a completely different approach with v2.

Instead of providing a series of customizable CompositionRoot classes -- which provided certain services, like logging, but also exposed various important properties, like  application and user configuration file folders -- everything is now a service accessible through an instance of `IHost`.

I think this simplifies how to use the library, and aligns it with the whole point of `IHost`, which is that it's the maestro directing how an app runs.

But the change will require rewrites. Most of those will be of the cut and paste variety. For example, rather than include `Autofac`-based dependency injection calls by overriding a protected virtual method call in a composition root class, you add an `Action<HostBuilderContext, ContainerBuilder>` to a configuration object.

After completing the specification of that configuration object you call an extension method, `Build()`, on it. The result, if everything goes right, is an instance of `IHost` containing not only your specific services but also a variety of other useful services:

- logging (via [J4JLogger](https://github.com/markolbert/J4JLogging))
- application and user configuration file management
- simple data protection (i.e., encryption and decryption of strings, via `Microsoft.AspNetCore.DataProtection`)
- command line parsing (via J4JCommandLine)
- dependency injection (via [Autofac](https://autofac.org/))
