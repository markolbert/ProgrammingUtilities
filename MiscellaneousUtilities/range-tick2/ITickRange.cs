#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'MiscellaneousUtilities' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;

namespace J4JSoftware.Utilities
{
    public interface ITickRange
    {
        bool IsSupported( object value );

        bool GetRange(int controlSize, object minValue, object maxValue, out object? result);
        List<object> GetRanges(int controlSize, object minValue, object maxValue);
        bool GetRange(int controlSize, int tickSize, object minValue, object maxValue, out object? result);
    }

    public interface ITickRange<in TValue, TResult> : ITickRange
        where TValue : IComparable
        where TResult : class
    {
        bool GetRange( int controlSize, TValue minValue, TValue maxValue, out TResult? result );
        List<TResult> GetRanges( int controlSize, TValue minValue, TValue maxValue );
        bool GetRange( int controlSize, int tickSize, TValue minValue, TValue maxValue, out TResult? result );
    }
}