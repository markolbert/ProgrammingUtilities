# J4JHostConfiguration: Configuring the Command Line Subsystem

- [Overview](#overview)
- [Command Line Application Configuration Files](#command-line-application-configuration-files)

## Overview

The command line parsing capabilities in `J4JHostConfiguration` are based on my [J4JCommandLine](https://github.com/markolbert/J4JCommandLine) library.

Command line processing depends on a number of configuration settings, each of which comes with operating system specific defaults that can be customized if necessary. The settings and the `J4JCommandLineConfiguration` extension methods used to customize them are:

|Setting|Extension Method|Purpose|
|-------|----------------|-------|
|Lexical elements|`LexicalElements`|Define the text elements which separate options, demarcate text, signal a new option definition, etc.|
|Text converters|`TextConverters`|Methods for converting command line text into C# values|
|Options generator|`OptionsGenerator`|Object that does the actual parsing of command line elements|
|Token cleaners|`TokenCleaners`|Object that cleans up after certain kinds of tokens are processed|
|Option definition|`OptionsInitializer`|Method that defines the command line options to be parsed|
|Command line application configuration file|`ConfigurationFileKeys`|Specifies which command line flag, if any, represents one or more application configuration file paths|

In the vast majority of cases only the last two methods -- `OptionsInitializer` and `ConfigurationFileKeys` -- are used, because the operating system based defaults for everything else are sufficient.

Here are the method signatures of the extension methods:

```csharp
public static J4JHostConfiguration OptionsInitializer(
    this J4JHostConfiguration config,
    Action<IOptionCollection> initializer );

public static J4JCommandLineConfiguration LexicalElements( 
    this J4JCommandLineConfiguration config,
    ILexicalElements tokens );

public static J4JCommandLineConfiguration TextConverters( 
    this J4JCommandLineConfiguration config,
    ITextConverters converters );

public static J4JCommandLineConfiguration OptionsGenerator( 
    this J4JCommandLineConfiguration config,
    IOptionsGenerator generator );

public static J4JCommandLineConfiguration TokenCleaners( 
    this J4JCommandLineConfiguration config,
    params ICleanupTokens[] tokenCleaners );

public static J4JCommandLineConfiguration OptionsInitializer(
    this J4JCommandLineConfiguration config,
    Action<OptionCollection> initializer );

public static J4JCommandLineConfiguration ConfigurationFileKeys(
    this J4JCommandLineConfiguration config,
    bool required,
    bool reloadOnChange,
    params string[] optionKeys );
```

## Command Line Application Configuration Files

There are situations where you want to be able to specify an application configuration file from the command line (this is particularly common with console applications). Unfortunately, this presents a bit of a chicken-and-egg problem when the `IConfiguration` API is involved. That's because the API assumes the paths to any required application configuration files are known before the API resolves information from various sources, including the command line, to target objects. Specifying an application configuration file on the command line breaks this requirement, since the file to be included in the `IConfiguration` process is only known after the `IConfiguration` process completes.

The solution to this I developed exercises the `IConfiguration` process twice, once with just the command line elements included and once with everything, including any command line specified configuration files, included. For this to work you need to specify which command line option represents the application configuration file. You do this via a call to the `J4JCommandLineConfiguration` extension method `ConfigurationFileKeys`:

```csharp
    public static J4JCommandLineConfiguration ConfigurationFileKeys(
        this J4JCommandLineConfiguration config,
        bool required,
        bool reloadOnChange,
        params string[] optionKeys
    )
    {
        config.ConfigurationFileKeys = new ConfigurationFileKeys( required, reloadOnChange, optionKeys );
        return config;
    }
```

You can specify more than one option key, whether the configuration file is required, and whether `IConfiguration` should reload if the file changes.

Be aware the option you use to hold configuration file path(s) must be bound to  either a string property, a string list or a string array. Any other property type will cause the associated command line values to be ignored.
