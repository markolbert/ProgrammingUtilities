# Dependency Injection

Provides a customized `IHostBuilder`-based system for producing an `IHost` instance which contain a number of useful services. These include:

- logging (via [J4JLogger](https://github.com/markolbert/J4JLogging))
- application and user configuration file management
- simple data protection (i.e., encryption and decryption of strings, via `Microsoft.AspNetCore.DataProtection`)
- command line parsing (via [J4JCommandLine](https://github.com/markolbert/J4JCommandLine))
- dependency injection (via [Autofac](https://autofac.org/))

This assembly targets Net 6 and has nullability enabled.

Please see the [github documentation](https://github.com/markolbert/ProgrammingUtilities/blob/master/docs/dependency/dependency.md) for details.
