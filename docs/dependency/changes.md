# Changes

|Version|Summary of Changes|
|-------|------------------|
|2.1|updated to Net 6|
|2.0|significant breaking changes; see below|

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
