# J4JCommandLine Subsystem

The command line parsing capabilities provided by the API are based on my [J4JCommandLine](https://github.com/markolbert/J4JCommandLine) library. There are various extension methods available for configuring how the subsystem works.

```csharp
public static J4JHostConfiguration OptionsInitializer(
    this J4JHostConfiguration config,
    Action<IOptionCollection> initializer )
```

Allows you to configure the `J4JCommandLine` subsystem, which parses command lines. See the [J4JCommandLine](https://github.com/markolbert/J4JCommandLine) documentation for more details.

```csharp
public static J4JHostConfiguration AddCommandLineAssemblies( this J4JHostConfiguration config, params Assembly[] cmdLineAssemblies )
```

Allows you to specify additional assemblies to search for capabilities extending the J4JCommandLine subsystem. If all you're using are the defaults this does not need to be called as the defaults are included automatically.
