#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// J4JDependencyInjectionException.cs
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

public class J4JDependencyInjectionException : Exception
{
    public J4JDependencyInjectionException(
        string message
    )
        : base( message )
    {
    }

    public J4JDependencyInjectionException(
        string message,
        Exception inner
    )
        : base( message, inner )
    {

    }

    public J4JDependencyInjectionException(
        string message,
        Exception inner,
        J4JHostConfiguration hostConfig
    )
        : base( message, inner )
    {
        HostConfiguration = hostConfig;
    }

    public J4JDependencyInjectionException(
        string message,
        J4JHostConfiguration hostConfig
    )
        : base( message )
    {
        HostConfiguration = hostConfig;
    }

    public J4JHostConfiguration? HostConfiguration { get; }
}
