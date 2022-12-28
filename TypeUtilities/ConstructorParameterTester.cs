// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of TypeUtilities.
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace J4JSoftware.DependencyInjection;

public class ConstructorParameterTester<T> : ITypeTester
    where T : class
{
    private readonly List<Type> _reqdParameters;

    public ConstructorParameterTester( params Type[] reqdParameters )
        :this( reqdParameters.AsEnumerable())
    {
    }

    public ConstructorParameterTester( IEnumerable<Type> reqdParameters )
    {
        _reqdParameters = reqdParameters.ToList();
    }

    public bool MeetsRequirements( Type toCheck )
    {
        if( !_reqdParameters.Any() )
            return true;

        var ctors = toCheck.GetConstructors( BindingFlags.Instance
                                           | BindingFlags.Public
                                           | BindingFlags.CreateInstance );

        foreach ( var ctor in ctors )
        {
            var ctorParameters = ctor.GetParameters();

            if( ctorParameters.Length != _reqdParameters.Count )
                continue;

            var allOkay = true;

            for( var idx = 0; idx < ctorParameters.Length; idx++ )
            {
                if( ctorParameters[ idx ].ParameterType.IsAssignableFrom( _reqdParameters[ idx ] ) )
                    continue;

                allOkay = false;
                break;
            }

            if( allOkay )
                return true;
        }

        return false;
    }
}