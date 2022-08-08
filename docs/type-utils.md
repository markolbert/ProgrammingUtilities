# Type Utilities

## Overview

I love [Autofac](https://www.autofac.org), a powerful dependency injection library for C#. I use it in almost all of my projects. It makes egistering concrete types as either specific interface implementations or as themselves a snap.

There are a few ways its use can be simplified, though, in some use cases. That's what this library aims to do: make it relatively straightforward to specify various kinds of type filter conditions, so you can register all the classes that match the conditions at once.

I've designed this library so that it is not dependent on Autofac. While I've only implemented its capabilities for Autofac, you are welcome to adapt them to whatever DI framework you use.

For details on how this library can be used with Autofac please see the documentation for my [DI library](dependency/dependency.md).

## Example

```csharp
private void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
{
    var typeTests = new TypeTests<IImportData>()
                    .AddTests( PredefinedTypeTests.NonAbstract )
                    .AddTests( PredefinedTypeTests.NonGeneric )
                    .AddTests( new FilterTypeTester<IImportData>( RequireSheetTypeAttribute ) );

    builder.RegisterTypeAssemblies<IImportData>( typeTests );
}

private bool RequireSheetTypeAttribute( Type toCheck ) =>
    toCheck.GetCustomAttribute<SheetTypeAttribute>( false ) != null;
```

This sequence will register all the concrete types in the assembly which defines the `IImportData` interface which:

- are not abstract
- are not generic
- are decorated with a non-inherited `SheeteTypeAttribute`

## Architecture

The tests are executed by an instance of `TypeTests`, which implements the `ITypeTester` interface. That interface contains a single method call, which returns true if a supplied type meets all the requirements and false if it doesn't:

```csharp
public interface ITypeTester
{
    bool MeetsRequirements( Type toTest );
}
```

`TypeTesters` implements the same interface, iterating over all the individual tests that were added to its instance. Those tests are added by calling one or more extension methods:

|Extension Method|Purpose|Arguments|
|----------------|-------|---------|
|AddPredefinedTests|adds one or more predefined tests (see below)|one or more predefined tests|
|HasConstructorArgs|requires a public instance (i.e., not-static) constructor taking a specified sequence of arguments|one or more types|
|DecoratedWith|requires the type be decorated with an attribute.<br><br>*You specify the attribute's type, so if you specify a type which isn't an attribute all types will be rejected/filtered out.*|the required attribute type, and a flag as to whether inherited attributes should be searched (defaults to false)|
|FilteredBy|requires a type to meet a user-defined test|a method reference which accepts a `Type` argument and returns true or false|
|AddTests|adds one or more instances of `ITypeTester`|allows the addition of custom-designed tests|

## Predefined Tests

There are a number of tests I use quite frequently so I created predefined tests that are characterized by the `PredefinedTypeTests` enum:

|Enum Value|Purpose|
|----------|-------|
|NonAbstract|requires that the type not be abstract|
|NonGeneric|requires that the type not be generic|
|ParameterlessConstructor|requires that the type have a public instance (i.e., non-static) constructor which accepts no arguments|
|OnlyJ4JLoggerRequired|requires that the type have a public instance (i.e., non-static) constructor which requires only one argument, an instance implementing my logging system, `IJ4JLogger` (for more information, see the IJ4JLogger documentation)|

## Creating Your Own Test

Creating a custom test is easy: just create a class which implements `ITypeTester`:

```csharp
public interface ITypeTester
{
    bool MeetsRequirements( Type toTest );
}
```
