﻿// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of WindowsAppUtilities.
//
// WindowsAppUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// WindowsAppUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with WindowsAppUtilities. If not, see <https://www.gnu.org/licenses/>.

using J4JSoftware.DependencyInjection;

namespace J4JSoftware.DeusEx;

public class J4JWinAppHostConfiguration : J4JHostConfiguration
{
    public J4JWinAppHostConfiguration(
        bool registerHost = true
    )
        : base( AppEnvironment.PackagedWinApp, registerHost )
    {
        ApplicationConfigurationFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
    }

    public override bool TryGetUserConfigurationFolder(out string? result)
    {
        result = ApplicationConfigurationFolder;
        return true;
    }
}