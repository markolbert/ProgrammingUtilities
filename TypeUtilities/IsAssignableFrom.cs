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

namespace J4JSoftware.DependencyInjection;

public class IsAssignableFrom<T> : ITypeTester
    where T : class
{
    public bool MeetsRequirements(Type toCheck) => typeof(T).IsAssignableFrom(toCheck);
}