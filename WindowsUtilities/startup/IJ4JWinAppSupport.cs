#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// IJ4JWinAppSupport.cs
//
// This file is part of JumpForJoy Software's WindowsUtilities.
// 
// WindowsUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// WindowsUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with WindowsUtilities. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.WindowsUtilities;

public interface IJ4JWinAppSupport
{
    bool IsInitialized { get; }
    string ConfigurationFilePath { get; }
    IServiceProvider? Services { get; }
    IDataProtector Protector { get; }
    ILoggerFactory? LoggerFactory { get; set; }
    ILogger? Logger { get; set; }
    AppConfigBase? AppConfig { get; }
}
