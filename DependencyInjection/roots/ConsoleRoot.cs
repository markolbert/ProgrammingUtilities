﻿// Copyright (c) 2021, 2022 Mark A. Olbert 
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
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection;

[ Obsolete( "Use J4JHostConfiguration IHostBuilder instead" ) ]
public class ConsoleRoot : CompositionRoot
{
    protected ConsoleRoot( string publisher,
        string appName,
        bool useConsoleLifetime = true,
        string? dataProtectionPurpose = null,
        string osName = "Windows",
        Func<Type?, string, int, string, string>? filePathTrimmer = null )
        : base( publisher, appName, dataProtectionPurpose, osName, filePathTrimmer )
    {
        UseConsoleLifetime = useConsoleLifetime;
    }

    public bool UseConsoleLifetime { get; }
    public override string ApplicationConfigurationFolder => Environment.CurrentDirectory;

    protected override bool Build()
    {
        // we need to update HostBuilder >>before<< the build actually takes place
        // because after the host is built HostBuilder will be null. That's why
        // we don't call the base method first
        if ( UseConsoleLifetime )
            HostBuilder!.UseConsoleLifetime();

        return base.Build();
    }
}