#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'WPFUtilities' is free software: you can redistribute it
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
using System.Windows.Xps.Serialization;

namespace J4JSoftware.WPFUtilities
{
    public interface IRangeCalculators
    {
        bool CalculateAlternatives<TValue>(
            TValue minValue,
            TValue maxValue,
            out List<RangeParameters<TValue>>? result,
            int minTickPowerOfTen = 2,
            MinorTickInfo[]? tickChoices = null );

        bool GetBestFit<TValue>(
            TValue minValue,
            TValue maxValue,
            out RangeParameters<TValue>? result,
            Func<int, int, int>? ranker = null,
            int minTickPowerOfTen = 2,
            MinorTickInfo[]? tickChoices = null );
    }
}