# IConfiguration Subsystem

These extension methods define the configuration files added to the `IConfiguration` subsystem and allow you to modify it before it gets built.

```csharp
public static J4JHostConfiguration AddApplicationConfigurationFile( 
    this J4JHostConfiguration config,
    string filePath,
    bool optional = true,
    bool reloadOnChange = false
    )

public static J4JHostConfiguration AddUserConfigurationFile(
    this J4JHostConfiguration config,
    string filePath,
    bool optional = true,
    bool reloadOnChange = false
        )
```

The builder allows you to incorporate as many JSON configuration files as you like, and to indicate which ones are application scope (i.e., stored in the application's directory) and which are user scope (i.e., stored in a user's settings). The files are used as sources for the `IConfiguration`-based services available from `IHost`.

I'm not sure if this means anything outside of the Windows environment. But it does come in handy there.

You can also indicate whether a given configuration file is required or optional (the default is optional), and whether or not the configuration system should watch the file and reload it if it changes.

```csharp
public static J4JHostConfiguration AddConfigurationInitializers(
    this J4JHostConfiguration config,
    params Action<IConfigurationBuilder>[] initializers)
```

Allows you to specify configuration actions that define the `IConfiguration`-based services exposed by the `IHost` instance. You would use this, for example, to add user secrets as a configuration source.
