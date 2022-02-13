using System;
using J4JSoftware.Configuration.CommandLine;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public interface IJ4JHost : IHost
    {
        string Publisher { get; }
        string ApplicationName { get; }

        string UserConfigurationFolder { get; }
        string ApplicationConfigurationFolder { get; }

        bool FileSystemIsCaseSensitive { get; }
        StringComparison CommandLineTextComparison { get; }
        ILexicalElements? CommandLineLexicalElements { get; }
        CommandLineSource? CommandLineSource { get; }
        OptionCollection? Options { get; }

        OperatingSystem OperatingSystem { get; }

        Func<bool> InDesignMode { get; }
    }
}
