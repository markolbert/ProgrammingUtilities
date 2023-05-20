# J4JMainWindowSupport

- [Properties](#properties)
- [Configuration](#configuration)
- [Methods](#methods)
- [Creation and Usage](#creation-and-usage)

`J4JMainWindowSupport` abstracts resizing a WinUI 3 app's main window on startup.

By working with `J4JWinAppSupport` it also does two other things:

- serializes the main window's size and position to the user configuration file, so the main window re-opens where and how the user left it when the app was last closed
- ensures the user configuration file is updated when each time the app is closed

## Properties

`J4JMainWindowSupport` has only one property:

|Property|Type|Comments|
|--------|----|--------|
|`AppWindow`|`AppWindow?`|the `AppWindow` associated with the main window|

`AppWindow` is exposed because locating it involves some method calls I always have to look up, so I figured it would be better to just put them in one place and forget about the details.

[return to top](#j4jmainwindowsupport)

[return to overview](startup.md)

[return to readme](readme.md)

## Configuration

You customize `J4JMainWindowSupport`'s behavior by overriding abstract methods in a derived class. Currently there is only one such method to define:

|Method|Returns|Comments|
|------|-------|--------|
|`GetDefaultWindowPositionAndSize()`|`RectInt32`|defines the default window size and position you want your app to have. This information is used only if a valid size and position cannot be determined from information in your app's configuration class (which must be derived from `AppConfigBase` for the overall process to work)|

[return to top](#j4jmainwindowsupport)

[return to overview](startup.md)

[return to readme](readme.md)

## Methods

`J4JMainWindowSupport` currently has only one method, used to set the initial size and position of your app's main window.

|Method|Returns|Argument(s)|Comments|
|------|-------|-----------|--------|
|`SetMainWindowPositionAndSize()`|`void`|sets the size and position of your app's main window|
|||`RectInt?` rect = null|the size and position for your main window. If null (the default), uses information from your application configuration file or the default size/position you defined|

[return to top](#j4jmainwindowsupport)

[return to overview](startup.md)

[return to readme](readme.md)

## Creation and Usage

You create an instance of `J4JMainWindowSupport` by passing the constructor two parameters:

|Parameter|Type|Comments|
|`mainWindow`|`Window`|your app's main window|
|`winAppSupport`|`IJ4JWinAppSupport`|an instance of your derived `J4JWinAppSupport` class|

You use `J4JMainWindowSupport` like this:

```csharp
public sealed partial class MainWindow
{
    private readonly MainWindowSupport _winSupport;

    public MainWindow()
    {
        this.InitializeComponent();

        _winSupport = new MainWindowSupport(this, App.Current.AppSupport);
        _winSupport.SetInitialWindowPositionAndSize();
    }
}
```

Note how the value of your derived `J4JWinAppSupport` class is retrieved from your app's `App` class.

[return to top](#j4jmainwindowsupport)

[return to overview](startup.md)

[return to readme](readme.md)
