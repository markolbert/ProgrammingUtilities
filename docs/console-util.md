## Console Utilities
Enables run-time editing of configuration parameters in console apps on an
as-needed basis.

This assembly targets Net5 and has nullability enabled.

#### Changes
|Version|Summary of Changes|
|-------|------------------|
|1.3|modified to reflect updated `J4JLogger` API|

#### Introduction
The Net5 `IConfiguration` system is highly flexible and can assemble
configuration information from many sources. However, when it's done
some values needed by a console app may still be undefined or invalid.
`ConfigurationUpdater<>` allows you to set updating rules for such
parameters and prompt the user for missing or replacement values.

You do this by defining `IPropertyUpdater<>` classes for the properties
you want to check/update:
```csharp
public class ProcessorTypeUpdater : PropertyUpdater<ProcessorType>
{
    public ProcessorTypeUpdater( IJ4JLogger? logger )
        : base( logger )
    {
    }

    public override UpdaterResult Update( ProcessorType origValue, out ProcessorType newValue )
    {
        newValue = origValue;

        if( origValue != ProcessorType.Undefined )
            return UpdaterResult.OriginalOkay;

        Console.WriteLine();
        Colors.WriteLine( "\nProcessorType".Yellow(), " is undefined\n" );

        newValue = Prompters.GetEnum<ProcessorType>(
            origValue,
            ProcessorType.Google,
            Enum.GetValues<ProcessorType>()
                .Where( x => GeoExtensions.IsSecuredProcessor( x ) )
                .ToList() );

        return UpdaterResult.Changed;
    }
}
```
The static class `Prompters` has a number of methods
(e.g., `GetEnum<>()` in the exampel) that simplify getting values.

After defining your property updaters you create one or more
`ConfigurationUpdater<>` classes to do the updating. You may want
more than one `ConfigurationUpdater<>` because your console app may
require different configuration rules depending upon how it's invoked.

Here's an example which does this using Autofac dependency injection:
```csharp
builder.RegisterType<ConfigurationUpdater<AppConfig>>()
    .OnActivating( x =>
    {
    x.Instance.Property( c => c.ProcessorType, new ProcessorTypeUpdater( CachedLogger ) );
    x.Instance.Property( c => c.APIKey, new APIKeyUpdater( CachedLogger ) );
    } )
    .Named<IConfigurationUpdater>( StoreKeyApp.AutofacKey )
    .SingleInstance();

builder.RegisterType<ConfigurationUpdater<AppConfig>>()
    .OnActivating( x =>
    {
        x.Instance.Property( c => c.ProcessorType, new ProcessorTypeUpdater( CachedLogger ) );
        x.Instance.Property( c => c.APIKey, new APIKeyUpdater( CachedLogger ) );
        x.Instance.Property( c => c.InputFile, new InputFileUpdater( CachedLogger ) );
    } )
    .Named<IConfigurationUpdater>( RouteApp.AutofacKey )
    .SingleInstance();
```
The `Property<,>()` method binds a particular updater to a particular
property. You can have multiple updaters bound to the same property.

**Important Note**: the order in which the updaters are bound using
`Property<,>()` can be important if some updaters rely on a valid value
existing for a particular configuration property. In the example I have
to bind `APIKeyUpdater` after `ProcessorTypeUpdater` because the logic
in `APIKeyUpdater` requires a valid `ProcessorType` property.

If you have multiple updaters registered with dependency injection 
you'll have to make sure you use the right one when you execute it.
With Autofac I generally use the `Named<>()` capability which lets
me call the correct updater like this:
```csharp
public class SomeApp : IHostedService
{
    internal const string AutofacKey = "RouteApp";

    private readonly IJ4JLogger _logger;

    public SomeApp(
        AppConfig config,
        IIndex<string, IConfigurationUpdater> configUpdaters,
        IJ4JLogger logger
    )
    {
        _logger = logger;
        _logger.SetLoggedType( GetType() );

        if( configUpdaters.TryGetValue( AutofacKey, out var updater )
                && updater.Update( _config ) )
        {
                // everything okay, proceed
                return;
        }

        _logger.Fatal( "Incomplete configuration, aborting" );
        _lifetime.StopApplication();
    }
}
```
