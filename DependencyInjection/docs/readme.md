# J4JSoftware.DependencyInjection

Provides a customized `IHostBuilder`-based system for producing an `IHost` instance which contains a number of useful services. These include:

- logging (via [J4JLogger](https://github.com/markolbert/J4JLogging))
- application and user configuration file management
- simple data protection (i.e., encryption and decryption of strings, via `Microsoft.AspNetCore.DataProtection`)
- command line parsing (via [J4JCommandLine](https://github.com/markolbert/J4JCommandLine))
- dependency injection (via [Autofac](https://autofac.org/))

This assembly targets Net 7 and has nullability enabled.

The repository is available online at [github](https://github.com/markolbert/ProgrammingUtilities/blob/master/docs/dependency/dependency.md).

- [Changes](changes.md)
- [Usage](usage.md)
- Examples
  - [Console app](console-app-example.md)
  - [WPF app](wpf-app-example.md)
