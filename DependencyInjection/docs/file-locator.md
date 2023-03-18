# File Locator API (Experimental)

- [Usage](#usage)
- [Background](#background)
- [What Constitutes a Match](#what-constitutes-a-match)
- [Configuration Methods](#configuration-methods)
- [Accessing Results](#accessing-results)
- [Secondary Paths](#secondary-paths)

## Usage

The following code will search `Environment.CurrentDirectory` and then `AppDomain.CurrentDomain.BaseDirectory` for files matching `ConfigurationFilePath`. The first matching file will end the search.

```csharp
var configLocator = new FileLocator( _logger )
                    .FileToFind( ConfigurationFilePath )
                    .Required()
                    .StopOnFirstMatch()
                    .ScanCurrentDirectory()
                    .ScanExecutableDirectory();

if( configLocator.Matches == 1 )
    return true;

_logger.Error( "Couldn't find app configuration file" );

return false;
```

## Background

I was not terribly happy with the earlier file locator code I'd written, so I developed a new, declarative API. It's experimental as of January, 2023, so you'll need to enable access to preview features to use it. This is done by adding the following XML to a **csproj** file:

```xml
<EnablePreviewFeatures>true</EnablePreviewFeatures>
```

## What Constitutes a Match

Because target files can be optional a match can occur whether or not a file is found. For optional files it's likely you'll want to specify that matches should be *writeable*. This means that if a file *can be created* in a directory, the directory will constitute a match *even if the file doesn't exist there*.

## Configuration Methods

The new declarative approach is based on creating an instance of `FileLocator`, optionally specifying an instance of `ILogger`, and then calling one or more of the following configuration methods on the instance. These are all static methods on `FileLocator` (the first argument, a `FileLocator` instance, is omitted for clarity):

|Method|Purpose|Comments|
|------|-------|--------|
|`FileSystemIsCaseSensitive()`|Declares file system to be case sensitive|The `FileLocator` constructor determines case-sensitivity automatically but you can override it.|
|`FileSystemIsNotCaseSensitive`|Declares file system to be case insensitive|The `FileLocator` constructor determines case-sensitivity automatically but you can override it.
|`FileToFind( string searchPath )`|Declares the file path to find|The path can be a simple file name or a relative file path.|
|`StopOnFirstMatch()`|Stop searching as soon as the first match is found|By default the API searches for all matches in the directories you specify.|
|`FindAllMatches()`|Find all matches (default)|By default the API searches for all matches in the directories you specify.|
|`FindAtMost( int maxMatches )`|Specify the maximum number of matches to find|By default the API searches for all matches in the directories you specify. This method lets you define a maximum.|
|`Required()'|A file matching the requirements must exist to satisfy the search||
|`Optional()`|A file matching the requirements need not exist to satisfy the search|default|
|`Readable()`|A file, if found, must be readable||
|`ReadabilityOptional()`|A file, if found, need not be readable|default|
|`Writeable()`|A file, if found, must be in a writeable directory||
|`WriteabilityOptional()`|A file, if found, need not be in a writeable directory|default|
|`ScanCurrentDirectory( IEnumerable<string>? secondaryPaths = null)`|Scan the directory specified by `Environment.CurrentDirectory` for matches|The optional enumerable secondary paths are searched after `Environment.CurrentDirectory`. See [Secondary Paths](#secondary-paths) for more details|
|`ScanExecutableDirectory( IEnumerable<string>? secondaryPaths = null)`|Scan the directory specified by `AppDomain.CurrentDomain.BaseDirectory` for matches|The optional enumerable secondary paths are searched after `Environment.CurrentDirectory`. See [Secondary Paths](#secondary-paths) for more details|
|`ScanDirectories( IEnumerable<string> dirPaths )`|Scan the directories specified by `dirPaths` for matches||
|`ScanDirectory( string primaryPath, IEnumerable<string>? secondaryPaths = null )`|Scan `primaryPath` for matches|The optional enumerable secondary paths are searched after `primaryPath`. See [Secondary Paths](#secondary-paths) for more details|

## Accessing Results

You can enumerate the results of scanning at any time by iterating over the `FileLocator` instance. Enumerating returns instances of `PathInfo` identifying matches:

```csharp
public class PathInfo
{
    public string Path { get; internal set; } = string.Empty;
    public PathState State { get; internal set; }  = PathState.None;
}
```

You can also retrieve the first match, if there is one, via the `FirstMatch` property of a `FileLocator` instance:

```csharp
public PathInfo? FirstMatch { get; }
```

## Secondary Paths

If a match is not found on a primary path, optional secondary paths are searched. How secondary paths interact with the primary path depends on whether or not a seconary path is *rooted*. From the [Microsoft documentation](https://learn.microsoft.com/en-us/dotnet/api/system.io.path.ispathrooted?view=net-7.0):

> A rooted path is file path that is fixed to a specific drive or UNC path; it contrasts with a path that is relative to the current drive or working directory. For example, on Windows systems, a rooted path begins with a backslash (for example, "\Documents") or a drive letter and colon (for example, "C:Documents").
>
> Note that rooted paths can be either absolute (that is, fully qualified) or relative. An absolute rooted path is a fully qualified path from the root of a drive to a specific directory. A relative rooted path specifies a drive, but its fully qualified path is resolved against the current directory.

If a secondary path is rooted the file name is extracted from the search path defined via `FileToFind()` and combined with the secondary path to define the path to search.

If a secondary path is *not* rooted it is combined with the entire search path.
