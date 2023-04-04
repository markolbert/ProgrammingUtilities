# J4JSoftware.DeusWinApp

The library repository is available on
[github](https://github.com/markolbert/ProgrammingUtilities/blob/master/DeusWinApp/docs/readme.md).

The change log is [available here](changes.md).

Extends `J4JDeusEx` to support Windows App (WinUI 3) programs, including
a customized `IHostBuilder`-based system for producing an `IHost` instance
which contains a number of useful services. These include:

- logging (via [J4JLogger](https://github.com/markolbert/J4JLogging))
- application and user configuration file management
- simple data protection (i.e., encryption and decryption of strings, via `Microsoft.AspNetCore.DataProtection`)
- command line parsing (via [J4JCommandLine](https://github.com/markolbert/J4JCommandLine))
- dependency injection (via [Autofac](https://autofac.org/))

The assembly also contains `J4JDeusExWinApp`, a class derived from `J4JDeusExHosted` which
provides **ViewModelLocator** functionality in Win3 app programs, and integrates
with my `J4JHostConfiguration` API.

For details on my `J4JHostConfiguration` API and `J4JDeusExHosted` please see
the [GitHub documentation](https://github.com/markolbert/ProgrammingUtilities/blob/master/DependencyInjection/docs/readme.md).

This assembly targets Net 7 and has nullability enabled.
