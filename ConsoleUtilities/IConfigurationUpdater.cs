namespace J4JSoftware.ConsoleUtilities
{
    public interface IConfigurationUpdater
    {
        bool Validate( object config );
    }

    public interface IConfigurationUpdater<in TConfig> : IConfigurationUpdater
        where TConfig: class
    {
        bool Validate( TConfig config );
    }
}