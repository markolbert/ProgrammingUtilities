using System;
using J4JSoftware.Configuration.CommandLine;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection.host
{
    public interface IJ4JHost : IHost
    {
        string Publisher { get; }
        string ApplicationName { get; }

        bool FileSystemIsCaseSensitive { get; }
        StringComparison CommandLineTextComparison { get; }
        ILexicalElements? CommandLineLexicalElements { get; }
        CommandLineSource? CommandLineSource { get; }
        
        OperatingSystem OperatingSystem { get; }

        Func<bool> InDesignMode { get; }
    }
}
