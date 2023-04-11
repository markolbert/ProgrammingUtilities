# J4JSoftware.MiscellaneousUtilities

|Version|Description|
|:-----:|-----------|
|2.4.1|fixed nuget dependencies|
|2.4.0|**breaking changes**, [see details below](#240)|
|2.3.0|moved file validation API to DependencyInjection project, [see details below](#230)|
|2.2.0|Improved how files are searched for when validating [see details below](#220)|
|2.1.1|Relocated exception formatting extension method `Exception.FormatException()`|
|2.1.0|Updated to Net 7, updated packages, [see details below](#210)|
|2.0|breaking changes; updated to Net 6|

## 2.4.0

To make the library more generally useful logging has been migrated from [Serilog](https://serilog.net/) to Microsoft's logging
system.

In general, this means instances of `ILoggerFactory` are used as construction parameters, rather than `ILogger`.
This is because, while Serilog lets you scope an `ILogger` instance to a new type, you can only define
the scope of a Microsoft `ILogger` by calling `ILoggerFactory.CreateLogger()`.

FWIW, in my projects I continue to use Serilog behind the scenes as my logging engine. It's great!

## 2.3.0

The file validation API was moved to my DependencyInjection project because it is needed there and I didn't want to force the inclusion of experimental APIs in such a commonly-used assembly.

## 2.2.0

*The following API was moved to my [DependencyInjection project](https://github.com/markolbert/ProgrammingUtilities/tree/master/DependencyInjection)*.

`FileExtensions.ValidateFilePath` now accepts an enumerable of paths where the file being validated might be located. It also supports logging via my `IJ4JLogger` system. All log events are set at the Verbose level.

`ValidateFilePath` looks for the required file as follows:

- if a required extension was specified and the path to check's extension doesn't match it or is missing, the path's extension is corrected
- if write access to the file *is not* required and the path exists, the result is set to the path and true is returned
- if write access *is* required and the path exists and is writeable, the result is set to the path and true is returned
- any specified folders are searched in the following sequence
  - if the path is not rooted (i.e., it's relative), the specified folders are searched using the relative path. If a match is found with the right access rights the result is set to its path and true is returned
  - if a file was not found, or if the path is rooted (i.e., it's absolute), the specified folders are searched for the file name component of the path alone. If a match is found with the right access rights the result is set to its path and true is returned
- if no match was found, the result is set to null and false is returned.

## 2.1.0

- Added extension method for extracting PropertyInfo from an Expression `GetPropertyInfo<TContainer, TProp>()`
- Added a static method for validating configuration and output files `FileExtensions.ValidateFilePath()`
- Added static hashing methods for strings, e.g., `CalculateHash()`
