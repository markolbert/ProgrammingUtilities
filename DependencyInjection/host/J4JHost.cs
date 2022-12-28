// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of DependencyInjection.
//
// DependencyInjection is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// DependencyInjection is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with DependencyInjection. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.Configuration.CommandLine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.DependencyInjection.host;

public sealed class J4JHost : IJ4JHost
{
    private readonly IHost _host;

    internal J4JHost( 
        IHost host,
        AppEnvironment appEnvironment
        )
    {
        _host = host;
        AppEnvironment = appEnvironment;

        Options = _host.Services.GetService<OptionCollection>();
    }

    public void Dispose() => _host.Dispose();

    public OptionCollection? Options { get; }
    public OperatingSystem OperatingSystem => Environment.OSVersion;
    public AppEnvironment AppEnvironment { get; }

    public Task StartAsync( CancellationToken cancellationToken = new CancellationToken() ) =>
        _host.StartAsync( cancellationToken );

    public Task StopAsync( CancellationToken cancellationToken = new CancellationToken() ) =>
        _host.StopAsync( cancellationToken );

    public IServiceProvider Services => _host.Services;

    public string Publisher { get; internal set; } = "undefined";
    public string ApplicationName { get; internal set; } = "undefined";
    public string UserConfigurationFolder { get; internal set; } = string.Empty;
    public List<string> UserConfigurationFiles { get; internal set; } = new();
    public string ApplicationConfigurationFolder { get; internal set; } = string.Empty;
    public List<string> ApplicationConfigurationFiles { get; internal set; } = new();

    public bool FileSystemIsCaseSensitive { get; internal set; }
    public StringComparison CommandLineTextComparison { get; internal set; }
    public ILexicalElements? CommandLineLexicalElements { get; internal set; }
    public CommandLineSource? CommandLineSource { get; internal set; }
}
