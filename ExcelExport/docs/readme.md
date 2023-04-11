# Excel Export

This assembly provides some wrappers around the [NPOI C# port](https://github.com/nissl-lab/npoi) which I've found useful.

This assembly targets Net 7 and has nullability enabled.

The library repository is available on [github](https://github.com/markolbert/ProgrammingUtilities/blob/master/EFCoreUtilities/docs/readme.md).

The change log is [available here](changes.md).

** Breaking change: logging was migrated to Microsoft Logging**

The starting point for using the library is creating an instance of `ExcelWorkbook`. It takes an optional `Func<IJ4JLogger>` logger factory. For more information on `IJ4JLogger` see the [github documentation](https://github.com/markolbert/J4JLogging).

Worksheets are created by callling the `AddWorksheet()` method on an `ExcelWorkbook` instance.

Tables are created by calling the `AddTable()` method on an `ExcelSheet` instance.
