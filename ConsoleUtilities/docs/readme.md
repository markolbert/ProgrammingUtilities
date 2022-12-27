# J4JSoftware.ConsoleUtilities

Enables run-time editing of configuration parameters in console apps on an as-needed basis.

This assembly targets Net 7 and has nullability enabled.

The library's repository is available on [github](https://github.com/markolbert/ProgrammingUtilities/blob/master/ConsoleUtilities/docs/readme.md).

The Net `IConfiguration` system is highly flexible and can assemble configuration information from many sources. However, when it's done some values needed by a console app may still be undefined or invalid. `ConfigurationUpdater<>` allows you to set updating rules for such parameters and prompt the user for missing or replacement values.

You do this by defining `IPropertyUpdater<>` classes for the properties you want to check/update.

The static class `Prompters` has a number of methods (e.g., `GetEnum<>()` in the exampel) that simplify getting values.

After defining your property updaters you create one or more `ConfigurationUpdater<>` classes to do the updating. You may want more than one `ConfigurationUpdater<>` because your console app may require different configuration rules depending upon how it's invoked.

The `Property<,>()` method binds a particular updater to a particular property. You can have multiple updaters bound to the same property.
