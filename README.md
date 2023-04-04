# Programming Utilities

A collection of utilities I find useful and which are used in [J4JSoftware](https://www.jumpforjoysoftware.com) projects.

-----
To make these libraries more generally useful, as of 2023 April 4, logging has been migrated
from [Serilog](https://serilog.net/) to Microsoft's logging system.

In general, this means instances of `ILoggerFactory` are used as construction parameters, rather than `ILogger`.
This is because, while Serilog lets you scope an `ILogger` instance to a new type, you can only define
the scope of a Microsoft `ILogger` by calling `ILoggerFactory.CreateLogger()`.

FWIW, in my projects I continue to use Serilog behind the scenes as my logging engine. It's great!

-----

## TL;DR

All these assemblies target Net7 and have nullability enabled.

All the assemblies are licensed under the GNU GPL-v3 (or later). See the [license file](LICENSE.md) for more details.

|Assembly|Focus|Nuget|
|-------------------|--------------------------------|-------------|
|[ConsoleUtilities](ConsoleUtilities/docs/readme.md)|as needed run-time parameter configuration|[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.ConsoleUtilities?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.ConsoleUtilities/)|
|[DependencyInjection](DependencyInjection/docs/readme.md)|general purpose composition root using Autofac and J4JLogging|[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.DependencyInjection?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.DependencyInjection/)|
|[DeusEx](DeusEx/docs/readme.md)|general purpose root composition object based on Autofac||
|[DeusWinApp](DeusWinApp/docs/readme.md)|J4JDeusEx extensions for Win3 apps||
|[EFCoreUtilities](EFCoreUtilities/docs/readme.md)|organized definition of table rules|[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.EFCore.Utilities?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.EFCore.Utilities/)|
|[ExcelExport](ExcelExport/docs/readme.md)|wrapper to simplify use of NPOI|[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.ExcelExport?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.ExcelExport/)|
|[MahAppsMaterialDesign](MahAppsMetroMaterialDesign/docs/readme.md)|add-ons for MahApps Material Design|[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.WPFUtilities.MaterialDesign?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.WPFUtilities.MaterialDesign/)|
|[MiscellaneousUtilities](MiscellaneousUtilities/docs/readme.md)|various relatively small APIs|[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.VisualUtilities?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.VisualUtilities/)|
|[TopologicalSort](TopologicalSort/docs/readme.md)|implementation of topological sort|[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.TopologicalSort?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.TopologicalSort/)|
|[TypeUtilities](TypeUtilities/docs/readme.md)|utilities for filtering types supporting my dependency injection library||
|[VisualUtilities](VisualUtilities/docs/readme.md)|utilities for dealing with images, media, etc.|[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.VisualUtilities?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.VisualUtilities/)|
|[WindowsUtilities](WindowsUtilities/docs/readme.md)|Utilities supporting Windows/Win3 apps||
|[WPFUtilities](WPFUtilities/docs/readme.md)|utilities for WPF applications|[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.WPFUtilities?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.WPFUtilities/)|
