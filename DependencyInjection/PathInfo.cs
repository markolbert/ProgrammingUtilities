#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// PathInfo.cs
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
using System.Runtime.Versioning;

namespace J4JSoftware.DependencyInjection;

public class PathInfo
{
    public string Path { get; internal set; } = string.Empty;
    public PathState State { get; internal set; }  = PathState.None;

    internal bool MeetsRequirements( FileLocator fileLoc )
    {
        foreach( var required in Enum.GetValues<PathState>() )
        {
            if( !fileLoc.RequiredPathState.HasFlag( required ) )
                continue;

            if( !State.HasFlag( required ) )
                return false;
        }

        return true;
    }
}
