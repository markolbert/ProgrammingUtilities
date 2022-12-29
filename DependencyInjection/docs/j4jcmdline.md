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

## Command Line Application Configuration Files

There are situations where you want to be able to specify an application configuration file from the command line (this is particularly common with console applications). Unfortunately, this presents a bit of a chicken-and-egg problem when the `IConfiguration` API is involved. That's because the API assumes the paths to any required application configuration files are known before the API resolves information from various sources, including the command line, to target objects. Specifying an application configuration file on the command line breaks this requirement, since the file to be included in the `IConfiguration` process is only known after the `IConfiguration` process completes.

The solution to this I developed exercises the `IConfiguration` process twice, once with just the command line elements included and once with everything, including any command line specified configuration files, included. For this to work you need to specify which command line option represents the application configuration file. You do this via a call to the `J4JCommandLineConfiguration` extension method `ConfigurationFileKeys`:

```csharp
    public static J4JCommandLineConfiguration ConfigurationFileKeys(
        this J4JCommandLineConfiguration config,
        bool required,
        bool reloadOnChange,
        params string[] optionKeys
    )
    {
        config.ConfigurationFileKeys = new ConfigurationFileKeys( required, reloadOnChange, optionKeys );
        return config;
    }
```

You can specify more than one option key, whether the configuration file is required, and whether `IConfiguration` should reload if the file changes.

Be aware the option you use to hold configuration file path(s) must be bound to  either a string property, a string list or a string array. Any other property type will cause the associated command line values to be ignored.
