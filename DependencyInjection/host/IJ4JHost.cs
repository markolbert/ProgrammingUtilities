using System;
using System.Collections.Generic;
using J4JSoftware.Configuration.CommandLine;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection;

public interface IJ4JHost : IHost
{
    string Publisher { get; }
    string ApplicationName { get; }

    string UserConfigurationFolder { get; }
    List<string> UserConfigurationFiles { get; }
    string ApplicationConfigurationFolder { get; }
    List<string> ApplicationConfigurationFiles { get; }

    bool FileSystemIsCaseSensitive { get; }
    StringComparison CommandLineTextComparison { get; }
    ILexicalElements? CommandLineLexicalElements { get; }
    CommandLineSource? CommandLineSource { get; }
    OptionCollection? Options { get; }

    OperatingSystem OperatingSystem { get; }

    AppEnvironment AppEnvironment { get; }
}