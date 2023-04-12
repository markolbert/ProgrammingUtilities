#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// TypeTester.cs
//
// This file is part of JumpForJoy Software's TypeUtilities.
// 
// TypeUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// TypeUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with TypeUtilities. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Linq;

namespace J4JSoftware.DependencyInjection;

public class TypeTester : ITypeTester
{
    public static TypeTester NonAbstract { get; } = new( x => !x.IsAbstract );
    public static TypeTester NonGeneric { get; } = new( x => !x.IsGenericType );
    public static TypeTester PublicConstructors { get; } = new( x => x.GetConstructors().Any() );

    private readonly Func<Type, bool>[] _testers;

    public TypeTester( params Func<Type, bool>[] testers )
    {
        _testers = testers;
    }

    public bool MeetsRequirements( Type toTest )
    {
        foreach( var tester in _testers )
        {
            if( !tester.Invoke( toTest ) )
                return false;
        }

        return true;
    }
}