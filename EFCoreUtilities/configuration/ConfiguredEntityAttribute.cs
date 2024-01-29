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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace J4JSoftware.EFCoreUtilities;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ConfiguredEntityAttribute : Attribute
{
    private readonly Type _configuratorType;

    public ConfiguredEntityAttribute( Type configuratorType, [ CallerMemberName ] string? decoratedClass = null )
    {
        if( !_configuratorType?.IsSubclassOf( typeof( EntityConfigurator<> ) ) ?? true )
            throw new InvalidCastException(
                $"Type {configuratorType} does not derive from {typeof( EntityConfigurator<> )}" );

        if( _configuratorType.GetConstructors( BindingFlags.CreateInstance | BindingFlags.Public )
                             .All( ctor => ctor.GetParameters().Length != 0 ) )
            throw new ArgumentException(
                $"Entity configuration type {_configuratorType} does not have a public parameterless constructor" );

        _configuratorType = configuratorType;

        try
        {
            EntityType = Type.GetType( decoratedClass! )!;
        }
        catch
        {
            throw new TypeAccessException( $"Could not find Type '{decoratedClass}'" );
        }
    }

    public Type EntityType { get; }

    public IEntityConfiguration GetConfigurator() =>
        (IEntityConfiguration) Activator.CreateInstance( _configuratorType )!;
}
