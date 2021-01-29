using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public interface IJ4JCompositionRootBase : IJ4JProtection
    {
        IHost? Host { get; }
        bool Initialized { get; }
        string ApplicationName { get; }
        string ApplicationConfigurationFolder { get; }
        string UserConfigurationFolder { get; }
        IJ4JLogger GetJ4JLogger();
        IJ4JProtection Protection { get; }
        void Initialize();
    }

    public interface IJ4JCompositionRoot : IJ4JCompositionRootBase
    {
        bool UseConsoleLifetime { get; set; }
    }

    public interface IJ4JViewModelLocator : IJ4JCompositionRootBase
    {
        bool InDesignMode { get; }
    }
}