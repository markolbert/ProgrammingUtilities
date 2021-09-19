# J4JLogging Subsystem

The logging features built into `IHost` use my [J4JLogging library](https://github.com/markolbert/J4JLogging). There are several extension methods available to configure it.

```csharp
public static J4JHostConfiguration FilePathTrimmer(
    this J4JHostConfiguration config, 
    Func<Type?, string, int, string, string> filePathTrimmer )
```

Sets the file path trimming method used by `J4JLogger`. This makes the displayed source code file path relative to the project folder, which generally improves the readability of the log events. See the [J4JLogger](https://github.com/markolbert/J4JLogging) documentation for details.

```csharp
public static J4JHostConfiguration LoggerInitializer(
    this J4JHostConfiguration config,
    Action<IConfiguration, J4JLoggerConfiguration> initializer )
```

Allows you to customize the configuration of `J4JLogger` before it is created. The host builder's `IConfiguration` instance is provided so you can, for example, take advantage of `Serilog`'s file-based configuration subsystem (`J4JLogger` wraps `Serilog`).

```csharp
public static J4JHostConfiguration AddNetEventSinkToLogger(
    this J4JHostConfiguration config,
    string? outputTemplate = null,
    LogEventLevel minimumLevel = LogEventLevel.Verbose
)
```

Allows you to add a `Serilog` sink to `J4JLogger` which will result in standard C# events being raised by `IJ4JLogger`. This is useful if you wish to display log events in a UI.
