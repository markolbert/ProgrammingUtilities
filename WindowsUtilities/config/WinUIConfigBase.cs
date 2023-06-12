﻿#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// WinUIConfigBase.cs
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

using J4JSoftware.EncryptedConfiguration;

namespace J4JSoftware.WindowsUtilities;

public class WinUIConfigBase : ConsoleAppConfig
{
    public new static string UserFolder { get; } = Windows.Storage.ApplicationData.Current.LocalFolder.Path;

    protected WinUIConfigBase()
    {
    }

    public PositionSize MainWindowRectangle { get; set; } = PositionSize.Empty;
}