using System;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.Configuration.CommandLine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.DependencyInjection.host;

public sealed class J4JHost : IJ4JHost
{
    private readonly IHost _host;

    internal J4JHost( IHost host )
    {
        _host = host;

        Options = _host.Services.GetService<OptionCollection>();
    }

    public void Dispose() => _host.Dispose();

    public Task StartAsync( CancellationToken cancellationToken = new CancellationToken() ) =>
        _host.StartAsync( cancellationToken );

    public Task StopAsync( CancellationToken cancellationToken = new CancellationToken() ) =>
        _host.StopAsync( cancellationToken );

    public IServiceProvider Services => _host.Services;

    public string Publisher { get; internal set; } = "undefined";
    public string ApplicationName { get; internal set; } = "undefined";
    public string UserConfigurationFolder { get; internal set; } = string.Empty;
    public string ApplicationConfigurationFolder { get; internal set; } = string.Empty;

    public bool FileSystemIsCaseSensitive { get; internal set; }
    public StringComparison CommandLineTextComparison { get; internal set; }
    public ILexicalElements? CommandLineLexicalElements { get; internal set; }
    public CommandLineSource? CommandLineSource { get; internal set; }
    public OptionCollection? Options { get; }

    public OperatingSystem OperatingSystem => Environment.OSVersion;

    public Func<bool> InDesignMode { get; internal set; } = () => false;
}
