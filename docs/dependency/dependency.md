# Dependency Injection

**There are breaking changes in this release.** Please re-review the documentation.

Provides a customized `IHostBuilder`-based system for producing an `IHost` instance which contain a number of useful services. These include:

- logging (via [J4JLogger](https://github.com/markolbert/J4JLogging))
- application and user configuration file management
- simple data protection (i.e., encryption and decryption of strings, via `Microsoft.AspNetCore.DataProtection`)
- command line parsing (via J4JCommandLine)
- dependency injection (via [Autofac](https://autofac.org/))

This assembly targets Net5 and has nullability enabled.

- [Changes](docs/dependency/changes.md)
- [Usage](docs/dependency/usage.md)
- Examples
  - [Console app](docs/dependency/console-app-example.md)
  - [WPF app](docs/dependency/wpf-app-example.md)
