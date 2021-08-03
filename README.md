## Programming Utilities
A collection of utilities used in [J4JSoftware](https://www.jumpforjoysoftware.com)
projects.

### Changes
There have been substantial changes since the last release, most notably to **TopologicalSort**
and **DependencyInjection**. Much of the functionality related to viewmodels previously provided
in *WPFViewModel* has been removed and replaced by a different approach added to **DependencyInjection**.
I've also added several new libraries. 

Please re-review the documentation for details.

### TL;DR
All these assemblies target Net5 and have nullability enabled.

All the assemblies are licensed under the GNU GPL-v3 (or later). See the 
[license file](LICENSE.md) for more details.


|Assembly|Focus|Nuget|
|-------------------|--------------------------------|-------------|
|[ConsoleUtilities](docs/console-util.md)|as needed run-time parameter configuration|[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.ConsoleUtilities?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.ConsoleUtilities/)|
|[DependencyInjection](docs/dependency.md)|general purpose composition root using Autofac and J4JLogging|[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.DependencyInjection?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.DependencyInjection/)|
|[EFCoreUtilities](docs/efcore.md)|organized definition of table rules|[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.EFCore.Utilities?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.EFCore.Utilities/)|
|[ExcelExport](docs/excel-export.md)|wrapper to simplify use of NPOI|[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.ExcelExport?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.ExcelExport/)|
|[MahAppsMaterialDesign](docs/mahappsmatdesign.md)|add-ons for MahApps Material Design|
|[MiscellaneousUtilities](docs/miscutils.md)|various relatively small APIs|
|[TopologicalSort](docs/topo-sort.md)|implementation of topological sort|[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.TopologicalSort?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.TopologicalSort/)|
|[VisualUtilities](docs/visual-utils.md)|utilities for dealing with images, media, etc.|[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.VisualUtilities?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.VisualUtilities/)|
|[WPFUtilities](docs/wpf-utilities.md)|utilities for WPF applications|

