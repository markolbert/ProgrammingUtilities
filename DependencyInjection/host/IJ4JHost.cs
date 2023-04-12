#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// IJ4JHost.cs
//
// This file is part of JumpForJoy Software's DependencyInjection.
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
#endregion

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