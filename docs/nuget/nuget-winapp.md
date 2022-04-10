# Windows App Utilities

Provides support utilities for Windows App (WinUI 3) programs, including
a customized `IHostBuilder`-based system for producing an `IHost` instance 
which contains a number of useful services. These include:

- logging (via [J4JLogger](https://github.com/markolbert/J4JLogging))
- application and user configuration file management
- simple data protection (i.e., encryption and decryption of strings, via `Microsoft.AspNetCore.DataProtection`)
- command line parsing (via [J4JCommandLine](https://github.com/markolbert/J4JCommandLine))
- dependency injection (via [Autofac](https://autofac.org/))

This assembly targets Net 6 and has nullability enabled.
