# J4JHostConfiguration: Configuring J4JLogging

- [Basic Configuration](#basic-configuration)
- [Adding a NetEventSink](#adding-a-neteventsink)
- [Trimming Source Code File Paths](#trimming-source-code-file-paths)

## Basic Configuration

`IJ4JHost` uses my [J4JLogging library](https://github.com/markolbert/J4JLogging), which is itself based on my favorite logging library, [Serilog](https://serilog.net/). Including logging is optional, and you configure it using several extension methods.

You configure your logging options by supplying a configuration method to `J4JHostConfiguration` through the `LoggerInitializer` extension method:

```csharp
public static J4JHostConfiguration LoggerInitializer( this J4JHostConfiguration config, 
Action<IConfiguration, J4JHostConfiguration, J4JLoggerConfiguration> initializer )
```

The initializer method receives instances of `IConfiguration`, `J4JHostConfiguration` and `J4JLoggerConfiguration`. Details on how to configure `ILogger` can be found in the [library's GitHub documentation](https://github.com/markolbert/J4JLogging).

## Adding a NetEventSink

In my Windows desktop apps, I often want to make some or all of the app's log messages available in the UI. `Serilog` doesn't directly support this, so far as I know, so I built it into `J4JLogger` as a custom `Serilog` sink called `NetEventSink`. You can add such a sink to an `IJ4JHost` instance by calling an extension method on `J4JHostConfiguration`:

```csharp
public static J4JHostConfiguration AddNetEventSinkToLogger(
    this J4JHostConfiguration config,
    string? outputTemplate = null,
    LogEventLevel minimumLevel = LogEventLevel.Verbose
)
```

`NetEventSink` allows you to filter what log events get sent to the UI sink based on `LogEventLevel`. It also allows you to define a different `Serilog` template for events sent to the UI sink. These are specified through the optional `outputTemplate` and `minimumLevel` method arguments.

## Trimming Source Code File Paths

One of the main additions to `Serilog` in `ILogger` (which was, in fact, the reason I wrote it) involves automatically adding source code annotations (e.g., source code file path, line number) to log events. Unfortunately, source code file paths typically get very long very quickly, so I implemented a way to shorten them.

It's done by supplying a method to trim file paths to `J4JHostConfiguration`. That's done by calling the `FilePathTrimmer` extension method:

```csharp
public static J4JHostConfiguration FilePathTrimmer(
    this J4JHostConfiguration config, 
    Func<Type?, string, int, string, string> filePathTrimmer )
```

The `Func<>` signature shows the parameters passed to the trimming function. There's a reference implementation contained in the `J4JLogging` library (it's defined in the static class `SourceCodeFilePathModifiers`) which I typically copy to each of my projects, and then supply to `J4JHostConfiguration`. It consists of two relatively simple methods, the first of which is the one supplied to `J4JHostConfiguration.FilePathTrimmer`:

```csharp
// copy these next two methods to the source code file where you configure J4JLogger
// and then reference FilePathTrimmer as the context converter you
// want to use
private static string FilePathTrimmer( Type? loggedType,
    string callerName,
    int lineNum,
    string srcFilePath )
{
    return CallingContextEnricher.DefaultFilePathTrimmer( loggedType,
                                                            callerName,
                                                            lineNum,
                                                            CallingContextEnricher.RemoveProjectPath( srcFilePath,
                                                                GetProjectPath() ) );
}

private static string GetProjectPath( [ CallerFilePath ] string filePath = "" )
{
    // DirectoryInfo will throw an exception when this method is called on a machine
    // other than the development machine, so just return an empty string in that case
    try
    {
        var dirInfo = new DirectoryInfo(System.IO.Path.GetDirectoryName(filePath)!);

        while (dirInfo.Parent != null)
        {
            if (dirInfo.EnumerateFiles("*.csproj").Any())
                break;

            dirInfo = dirInfo.Parent;
        }

        return dirInfo.FullName;
    }
    catch (Exception)
    {
        return string.Empty;
    }
}
```
