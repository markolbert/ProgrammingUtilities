#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ITickRange.cs
//
// This file is part of JumpForJoy Software's MiscellaneousUtilities.
// 
// MiscellaneousUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// MiscellaneousUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with MiscellaneousUtilities. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace J4JSoftware.Utilities;

[RequiresPreviewFeatures("Experimental, subject to change or removal")]
public interface ITickRange
{
    bool IsSupported( Type toCheck );
    bool IsSupported<T>();

    bool Configure( ITickRangeConfig config );

    bool GetRange( double controlSize, object minValue, object maxValue, out object? result );
    List<object> GetRanges( double controlSize, object minValue, object maxValue );
    bool GetRange( double controlSize, int tickSize, object minValue, object maxValue, out object? result );
}

[RequiresPreviewFeatures("Experimental, subject to change or removal")]
public interface ITickRange<in TValue, TResult> : ITickRange
    where TValue : IComparable
    where TResult : class
{
    bool GetRange( double controlSize, TValue minValue, TValue maxValue, out TResult? result );
    List<TResult> GetRanges( double controlSize, TValue minValue, TValue maxValue );
    bool GetRange( double controlSize, int tickSize, TValue minValue, TValue maxValue, out TResult? result );
}