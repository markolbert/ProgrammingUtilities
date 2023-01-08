# J4JSoftware.DependencyInjection

|Version|Description|
|:-----:|-----------|
|2.3.3|apply validation/searching functionality when adding application configuration files, [see details below](#233)|
|2.3.2|fix bug causing app config files to be ignored|
|2.3.1|make file system case sensitivity configurable but set by default|
|2.3.0|Updated to Net 7, updated packages, [see additional details below](#230)|
|2.2|simplified creating type tests, register IJ4JHost when creating IJ4JHostConfiguration|
|2.1|updated to Net 6|
|2.0|significant breaking changes; [see details below](#200)|

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
