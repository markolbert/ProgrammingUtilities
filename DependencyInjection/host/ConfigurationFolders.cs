﻿#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// 
// This file is part of J4JLogger.
//
// J4JLogger is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JLogger is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JLogger. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.IO;

namespace J4JSoftware.DependencyInjection;

public record ConfigurationFolders( string Publisher,
    string ApplicationName,
    Func<bool>? InDesignMode )
{
    public string ApplicationConfigurationFolder =>
        InDesignMode?.Invoke() ?? false
            ? AppContext.BaseDirectory
            : Environment.CurrentDirectory;

    public string UserConfigurationFolder =>
        Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                      Publisher,
                      ApplicationName );
}