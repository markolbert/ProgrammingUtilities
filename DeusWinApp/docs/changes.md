# J4JSoftware.WindowsAppUtilities

|Version|Description|
|:-----:|-----------|
|1.1.1|fixed nuget dependencies|
|1.1.0|**breaking changes**, [see details below](#110)|
|1.0.0|Updated to Net 7, updated packages|

## 1.1.0

To make the library more generally useful logging has been migrated from [Serilog](https://serilog.net/) to Microsoft's logging
system.

In general, this means instances of `ILoggerFactory` are used as construction parameters, rather than `ILogger`.
This is because, while Serilog lets you scope an `ILogger` instance to a new type, you can only define
the scope of a Microsoft `ILogger` by calling `ILoggerFactory.CreateLogger()`.

FWIW, in my projects I continue to use Serilog behind the scenes as my logging engine. It's great!
