#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// EntityConfigurationAttribute.cs
//
// This file is part of JumpForJoy Software's EFCoreUtilities.
// 
// EFCoreUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// EFCoreUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with EFCoreUtilities. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.EFCoreUtilities;

[ AttributeUsage( AttributeTargets.Class, Inherited = false ) ]
public class EntityConfigurationAttribute : Attribute
{
    private readonly Type _configType;

    public EntityConfigurationAttribute( Type configType, Type? contextType = null )
    {
        ContextType = contextType;

        if( contextType != null )
        {
            if( !contextType.IsAssignableTo( typeof( DbContext ) ) )
                throw new ArgumentException($"Type {contextType.Name} is not assignable to {typeof(DbContext)}");
        }

        _configType = configType ?? throw new NullReferenceException( nameof( configType ) );

        if( !typeof( IEntityConfiguration ).IsAssignableFrom( configType ) )
            throw new
                ArgumentException( $"Database entity configuration type is not {nameof( IEntityConfiguration )}" );
    }

    public Type? ContextType { get; }

    public IEntityConfiguration GetConfigurator()
    {
        return (IEntityConfiguration) Activator.CreateInstance( _configType )!;
    }
}