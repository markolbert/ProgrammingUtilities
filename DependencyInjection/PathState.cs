#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// PathState.cs
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

namespace J4JSoftware.DependencyInjection;

[Flags]
public enum PathState
{
    Exists = 1 << 0,
    Readable = 1 << 1,
    // ReSharper disable once IdentifierTypo
    Writeable = 1 << 2,

    None = 0,
    All = Exists | Readable | Writeable
}
