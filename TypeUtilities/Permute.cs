#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// Permute.cs
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

using System.Collections.Generic;

namespace J4JSoftware.DependencyInjection;

// thanx to https://gist.github.com/fdeitelhoff/5052484 for this!
public static class PermuteExtensions
{
    public static IEnumerable<IEnumerable<T>> Permute<T>( this IList<T> values )
    {
        ICollection<IList<T>> result = new List<IList<T>>();

        Permute( values, values.Count, result );

        return result;
    }

    private static void Permute<T>( IList<T> values, int n, ICollection<IList<T>> result )
    {
        if( n == 1 )
            result.Add( new List<T>( values ) );
        else
        {
            for( var i = 0; i < n; i++ )
            {
                Permute( values, n - 1, result );
                Swap( values, n % 2 == 1 ? 0 : i, n - 1 );
            }
        }
    }

    private static void Swap<T>( IList<T> values, int i, int j ) =>
        ( values[ i ], values[ j ] ) = ( values[ j ], values[ i ] );
}