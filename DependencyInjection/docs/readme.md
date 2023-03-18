# J4JSoftware.DependencyInjection

|API|Description|
|---|-----------|
|[J4JHostConfiguration](#j4jhostconfiguration)|An extended/enhanced version of the `IHost` interface. It is incorporated into my [J4JDeusEx](j4jdeusex.md) API to provide a *ViewModelLocator* capability for most common Windows runtime environments (e.g., console, WPF, Windows Applications aka WinApp3)|
|[FileUtilities](#fileutilities)|Provides a static method for searching for and validating files|
|[FileLocator](file-locator.md)|Declarative API for locating files *experimental*|

## J4JHostConfiguration

The `J4JHostConfiguration` API allows you to create objects implementing the `IJ4JHost` interface. That interface extends Microsoft's `IHost` interface to include:

- the app publisher's name;
- the application name;
- the path to where user configuration files are stored (and the paths to individual user configuration files);
- the path to where application configuration files are storedf (and the paths to individual application configuration files);
- a flag indicating whether or not the operating system the code is running on has a case sensitive filesystem;
- various items related to how the command line is parsed:
  - the `StringComparison` object used to compare text entered on the command line to programmatic values;
  - (optional) the text tokens that serve as lexical elements (e.g., the key prefix, the character(s) used to demarcate strings, etc.) on the command line;
  - the options defined for command line processing;
- the operating system the code is running on; and,
- the environment in which the code is running (e.g., console, WPF, WPF in design mode, etc.)

The `IJ4JHost` instance provides a number of useful services:

- (optional) logging (via [J4JLogger](https://github.com/markolbert/J4JLogging))
- simple data protection (i.e., encryption and decryption of strings, via `Microsoft.AspNetCore.DataProtection`)
- (optional) command line parsing (via [J4JCommandLine](https://github.com/markolbert/J4JCommandLine))
- dependency injection (via [Autofac](https://autofac.org/))

This assembly targets Net 7 and has nullability enabled.

The repository is available online at [github](https://github.com/markolbert/ProgrammingUtilities/blob/master/DependencyInjection/docs/readme.md).

- [Changes](changes.md)
- [Usage](usage.md)
- [Integration with J4JDeusEx](j4jdeusex.md)
- Examples
  - [Console app (not sandboxed)](console-app-example.md)
  - [Desktop app (sandboxed and not sandboxed)](wpf-app-example.md)

*Sandboxed* refers to whether or not the app has, potentially, unfettered access to the filesystem. Modern desktop apps (e.g., Windows Application v3 aka WinApp3, and, I think, UWP) are sandboxed, at least so far as storing application data is concerned. Older desktop environments (e.g., Windows Forms, WPF) are not sandboxed.

## FileUtilities

`FileExtensions.ValidateFilePath` validates the existence and, optionally, writeability, of a file. It can search specified alternative folders for the file if it is not found on the original path that's provided. It also supports logging via my `ILogger` system. All log events are set at the *Verbose* level.

```csharp
  public static bool ValidateFilePath(
      string path,
      out string? result,
      string? reqdExtension = ".json",
      IEnumerable<string>? folders = null,
      bool requireWriteAccess = false,
      ILogger? logger = null
  )
```

|Argument|Description|
|--------|-----------|
|`path`|the file path to check|
|`result`|the path to the file if it was found, or null if it was not|
|`reqdExtension`|the required file extension (optional; default = **.json**). If specified, `path` will be forced to match it.|
|`folders`|a list of folders to search for the file if it isn't found at `path` (optional). See below for details.|
|`requireWriteAccess`|require that `path` be writeable. This is verified by testing to see if a temporary file can be written to the folder where the file was found (the temporary file is deleted after the test).|
|`logger`|an instance of `ILogger` to record log events (optional). All log events are marked at the *Verbose* level|

`ValidateFilePath` looks for the required file as follows:

- if write access to `path` *is not* required and `path` exists, `result` is set to `path` and `true` is returned
- if write access *is* required and `path` exists and is writeable, `result` is set to `path` and `true` is returned
- any specified folders are searched in the following sequence
  - if `path` is not rooted (i.e., it's relative), the specified folders are searched using the relative path. If a match is found and writeability matches what was specified, `result` is set to the matching path and `true` is returned
  - if `path` was not found, or if `path` is rooted (i.e., it's absolute), the specified folders are searched for the **file name component** of `path` alone. If a match is found and writeability matches what was specified, `result` is set to the matching path and `true` is returned
- if no match is found, `result` is set to `null` and `false` is returned.
