# WinUI 3 Startup Support

There are several things I find myself coding in almost every WinUI 3 app I write:

- interacting with a per-user application configuration file (i.e., so each user can have their own configuration)
- overriding WinUI 3's default window sizing and initial location logic
- configuring `IServiceProvider` through the `IHostBuilder`/`IHost` API
- configuring logging (I love `Serilog`, but I like to access it through the Microsoft `ILoggerFactory`/`ILogger` API so people can use their own favorite logging system when they use my libraries)

The support classes described here provide a generalized way of doing all this so I don't have to rewrite the same code for each WinUI 3 app.

- [J4JWinAppSupport](j4jwinappsupport.md)
  - [Properties](j4jwinappsupport.md#properties)
  - [Configuration](j4jwinappsupport.md#configuration)
  - [Creation and Usage](j4jwinappsupport.md#creation-and-usage)
- [Main window support](j4jmainwindowsupport.md)
  - [Properties](j4jmainwindowsupport.md#properties)
  - [Configuration](j4jmainwindowsupport.md#configuration)
  - [Methods](j4jmainwindowsupport.md#methods)
  - [Creation and Usage](j4jmainwindowsupport.md#creation-and-usage)

[return to readme](readme.md)
