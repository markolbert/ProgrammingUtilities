# Entity Framework Utilities

## Changes

|Version|Summary of Changes|
|-------|------------------|
|1.2|revised design time factory|
|1.1|updated to Net 6|
|1.0.1|added nuget readme|
|1.0|initial release|

This assembly provides two capabilities to simplify using Entity Frameowork Core in projects:

- Simplifies setting up a **design-time factory** for instances of `DbContext` to use during development.
- Provides a way of associating **entity configuration information** with the entity being configured when you're using a fluent design approach.

This assembly targets Net5 and has nullability enabled.

## Design-time factory

Debugging a `DbContext` can be complicated if things like the location of the database is different at design time. I often find that's the case in my projects because a configuration class may be defined in its own assembly so it can be shared across multiple projects.

`DesignTimeFactory<TDbContext>` abstracts this process. All you need to do is implement a simple class deriving from 
`DesignTimeFactory<TDbContext>`. Here's an example of doing this when you're using Sqlite3:

```csharp
public class QbDbDesignTimeFactory : DesignTimeFactory<QbDbContext>
{
    protected override void ConfigureOptionsBuilder( DbContextOptionsBuilder<QbDbContext> builder, string dbDirectory )
    {
        var connBuilder = new SqliteConnectionStringBuilder()
        {
            DataSource = Path.Combine(dbDirectory, "QbDatabase.db")
        };

        builder.UseSqlite(connBuilder.ConnectionString);
    }
}
```

All the derived class does is define the path to the database via a connection string.
You could make other changes to the connection string at this point, too.

You use this with the **ef tools** like this:

- within the *Package Manager Console*, move into the directory where your database
assembly is defined
- if needed, create a new migration
- run the update command specifying the path to where the Sqlite3 db file should
be created

If the database is defined in a folder called **QbDatabase** and the consuming
assembly is called **ImportNames** this would look like this (the example starts
off by printing out the current working directory, which I generally find to be a
good cautionary step to take):

```
PM> pwd

Path                                    
----                                    
C:\Programming\Quickbooks2LGL\QbDatabase


PM> dotnet ef migrations add Initial
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
PM> dotnet ef database update -- ../ImportNames
Build started...
Build succeeded.
Applying migration '20220802204340_Initial'.
Done.
PM> 
```

If you don't include the path to where you want the database file created it will
be created in the directory defining the database.

## Entity Configuration Information

Database entities almost always require you configure the database traits of properties (e.g., is that string nullable or not?), relationships, etc.

EF Core lets you do this in at least two ways, one by decorating entity classes and their properties with attributes and one by calling various fluent design configuration methods (you can mix and match, too,
I think).

For various reasons I strongly prefer exclusively using the fluent design approach, rather than the attribute-based approach. But by default that would result in a very large `OnModelCreating()` method override in my `DbContext`-derived class.

That's messy. So I wrote several classes to allow me to put the necessary fluent design method calls in a class I associate with an entity class. Each entity gets its own configuration class. And a single extension method call in `OnModelCreating()` knits it all together:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.ConfigureEntities(this.GetType().Assembly);
}
```

Here's an example of how the support classes are used:

```csharp
[EntityConfiguration(typeof(QuestionDbConfigurator))]
public class QuestionDb
{
    public int ID { get; set; }
    public string SurveyPlanetID { get; set; }
    public string Text { get; set; }
    public string Type { get; set; }
    public bool Required { get; set; }

    public List<AnswerDb> Answers { get; set; }
    public List<QuestionChoiceDb> Choices { get; set; }

    public int SurveyID { get; set; }
    public SurveyDb Survey { get; set; }
}

internal class QuestionDbConfigurator : EntityConfigurator<QuestionDb>
{
    protected override void Configure( EntityTypeBuilder<QuestionDb> builder )
    {
        builder.HasOne(x => x.Survey)
            .WithMany(x => x.Questions)
            .HasPrincipalKey(x => x.ID)
            .HasForeignKey(x => x.SurveyID);

        builder.HasIndex( x => x.SurveyPlanetID )
            .IsUnique();
    }
}
```

`QuestionDbConfigurator` holds the fluent-design configuration information for the entity class `QuestionDb`. It's internal because only the `DbContext`-derived class in the database assembly needs access to it.

The `EntityConfigurationAttribute` attribute decorating `QuestionDb` lets the utility code know where to get the configuration information for `QuestionDb`.
