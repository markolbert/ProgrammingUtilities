# J4JSoftware.DependencyInjection

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
- Examples
  - [Console app](console-app-example.md)
  - [WPF app](wpf-app-example.md)
