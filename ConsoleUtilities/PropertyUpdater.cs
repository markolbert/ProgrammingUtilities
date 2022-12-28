// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of ConsoleUtilities.
//
// ConsoleUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// ConsoleUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with ConsoleUtilities. If not, see <https://www.gnu.org/licenses/>.

using System;
using J4JSoftware.Logging;

namespace J4JSoftware.ConsoleUtilities;

public abstract class PropertyUpdater<TProp> : IPropertyUpdater<TProp>
{
    protected PropertyUpdater( IJ4JLogger? logger )
    {
        Logger = logger;
        Logger?.SetLoggedType( GetType() );
    }

    protected IJ4JLogger? Logger { get; }

    public abstract UpdaterResult Update( TProp? origValue, out TProp? newValue );

    public Type ValidatorType => typeof( TProp );

    UpdaterResult IPropertyUpdater.Update( object? origValue, out object? newValue )
    {
        newValue = origValue;

        if( origValue is TProp castValue )
        {
            var result = Update( castValue, out var innerNew );

            if( result == UpdaterResult.Changed )
                newValue = innerNew;

            return result;
        }

        Logger?.Error( "Expected a {0} but got a {1}", typeof( TProp ), origValue?.GetType() );

        return UpdaterResult.InvalidValidator;
    }
}