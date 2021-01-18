namespace J4JSoftware.ConsoleUtilities
{
    public interface IConfigurationUpdater
    {
        bool Update( object config );
    }

    public interface IConfigurationUpdater<in TConfig> : IConfigurationUpdater
        where TConfig: class
    {
        bool Update( TConfig config );
    }
}