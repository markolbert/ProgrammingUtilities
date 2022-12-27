# J4JSoftware.MiscellaneousUtilities

The library repository is available on [github](https://github.com/markolbert/ProgrammingUtilities/blob/master/docs/miscutils/miscutils.md).

The change log is [available here](changes.md).

This assembly contains a few miscellaneous utility methods I've found useful:

- Extension method for extracting PropertyInfo from an Expression `GetPropertyInfo<TContainer, TProp>()`
- Static method for validating configuration and output files `FileExtensions.ValidateFilePath()`
- Static hashing methods for strings, e.g., `CalculateHash()`

## Experimental APIs

This assembly also provides two APIs which I've found helpful in building WPF apps, particularly ones that utilize [SyncFusion's WPF controls](https://www.syncfusion.com/wpf-controls):

| API | Description |
|-----|-------------|
|[SelectableNode](selectable/intro.md)|provides a wrapper for selecting nodes that are used in a tree control|
|[RangeCalculator](range-tick/intro.md)|provides a means for selecting a reasonable pair of major/minor tick intervals for a range control|

This assembly targets Net 7 and has nullability enabled.

**Both of these APIs are experimental and subject to change/removal.**

Examples of using both APIs are forthcoming.
