# J4JHost: Integration with J4JDeusEx

- [Background](#background)
- [J4JDeusEx Structure](#j4jdeusex-structure)
- [Initializing J4JDeusEx](#initializing-j4jdeusex)
- Examples
  - [Console App (not sandboxed)](console-app-example.md)
  - [Desktop App (sandboxed and not sandboxed)](win-desktop-app-example.md)
  
## Background

While the `J4JHost` system allows you to create an `IHost`-based application controller with a variety of useful features (e.g., logging, command line processing) it's more useful, as is, in simple console apps than Windows desktop apps. The reason has to do with the consequences of not having built-in support for dependency injection.

In a simple console app there's typically only one "thing" running at a time. Incorporating dependency injection is relatively straightforward, because generally the only special case of creating an object on the fly you need to deal with is obtaining that very first singleton application controller. Once you've acquired it, most console app architectures simply explicitly create the objects they need as they need them, based on information local to the code that's creating them. Or so it seems to be in my simple console apps; your mileage may differ.

Windows desktop apps are quite different. It's very hard, if possible at all, to create a single root application controller and have everything get created locally on demand. It has to do with the potential multiplicity of ways in which code, and the objects that code requires, may be activated.

A similar thing happens in AspNetCore applications as well. But AspNetCore has built-in support for dependency injection. You can define your objects using constructor parameters that have to be created on the fly and, provided you've registered the parameter types with the dependency injection framework, be assured things will just work.

I'm not aware of any Windows desktop architecture that contains the same kind of built-in support for dependency injection. Windows Forms doesn't have it. WPF doesn't have it. Windows App v2 doesn't have it. Windows App v3 doesn't have it, although I think it's on the roadmap to be added. It's possible UWP has it; I've never done any work in UWP.

The net result is that, to use dependency injection in most or all Windows desktop architectures, you need to use some kind of **ViewModelLocator** pattern: a class with static methods which can be called to create objects registered with the dependency injection system on demand.

`J4JDeusEx` is my attempt at creating a generalized ViewModelLocator object that integrates with my `J4JHost` API so I can have a uniform way of interacting with dependency injection regardless of whether I'm writing a console app or a Windows desktop app.

## J4JDeusEx Structure

`J4JDeusEx` has a pretty simple public structure, all of which is static:

```csharp
public class J4JDeusEx
{
    public static IServiceProvider ServiceProvider { get; protected set; }
    public static bool IsInitialized { get; protected set; }
    public static string? CrashFilePath { get; protected set; }
    public static IJ4JLogger? Logger { get; protected set; }

    public static void OutputFatalMessage( string msg, IJ4JLogger? logger );
}
```

- `ServiceProvider` is your access point for dependency injection operations.
- `IsInitialized` indicates whether `J4JDeusEx` has been properly initialized (if you access the `ServiceProvider` property when it isn't correctly initialized you'll trigger an exception).
- `CrashFilePath` is the path to a file that holds messages if something goes wrong during `J4JDeusEx` initialization. It's nullable because the path gets defined during initialization.
- `Logger` is an instance of `J4JLogger`, provided you configured one when setting up `J4JHostConfiguration` and `J4JDeusEx` initialization succeeded.

But given that all the properties are static, with protected setters, how do they get set during initialization?

## Initializing J4JDeusEx

The answer is, they get set when you call a non-static public initialization method on a class derived from `J4JDeusEx`.

There are two such classes, one for sandboxed environments (i.e., where the app does not have unfettered access to the file system), and one for non-sandboxed environments (i.e., where the app can, potentially, access any part of the file system). Windows Forms, WPF, and console apps are examples of non-sandboxed environments. Windows Applications v3 and UWP (and WinRT, I suspect) are examples of sandboxed environments.

|Class|Assembly|Target Environment(s)|GitHub Repository|
|-------------|--------|---------------------|-----------------|
|J4JDeusEx|J4JSoftware.J4JDeusEx|n/a (base class)|[repository](https://github.com/markolbert/ProgrammingUtilities/blob/master/DeusEx/docs/readme.md)|
|J4JDeusExHosted|J4JSoftware.DependencyInjection|non-sandboxed apps (e.g., Windows Forms, WPF, console apps)|[repository](https://github.com/markolbert/ProgrammingUtilities/blob/master/DependencyInjection/docs/readme.md)|
|J4JDeusExWinApp|J4JSoftware.WindowsAppUtilities|sandboxed apps (e.g., Windows Application v3, UWP)|[repository](https://github.com/markolbert/ProgrammingUtilities/blob/master/WindowsAppUtilities/docs/readme.md)|

`J4JDeusExHosted` is integrated into the `J4JHost` system by virtue of an abstract protected method which retrieves an instance of `J4JHostConfiguration`.
