# Entity Framework Utilities

This assembly provides two capabilities to simplify using Entity Frameowork Core in projects:

- Simplifies setting up a **design-time factory** for instances of `DbContext` to use during development.
- Provides a way of associating **entity configuration information** with the entity being configured when you're using a fluent design approach.

This assembly targets Net5 and has nullability enabled.

For more information consult the [github documentation](https://github.com/markolbert/J4JCommandLine).

## Design-time factory

Debugging a `DbContext` can be complicated if things like the location of the database is different at design time. I often find that's the case in my projects because a configuration class may be defined in its own assembly so it can be shared across multiple projects.

`DesignTimeFactory<TDbContext>` abstracts this process. All you need to do is implement an abstract method, `GetDatabaseConfiguration()`, which returns an `IDbContextFactoryConfiguration`. You'll also need to write a simple class that implements `IDbContextFactoryConfiguration`.

## Entity Configuration Information

Database entities almost always require you configure the database traits of properties (e.g., is that string nullable or not?), relationships, etc.

EF Core lets you do this in at least two ways, one by decorating entity classes and their properties with attributes and one by calling various fluent design configuration methods (you can mix and match, too,
I think).

For various reasons I strongly prefer exclusively using the fluent design approach, rather than the attribute-based approach. But by default that would result in a very large `OnModelCreating()` method override in my `DbContext`-derived class.

That's messy. So I wrote several classes to allow me to put the necessary fluent design method calls in a class I associate with an entity class. Each entity gets its own configuration class. And a single extension method call in `OnModelCreating()` knits it all together.
