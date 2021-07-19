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

namespace J4JSoftware.WPFUtilities
{
    public record CustomTickManager<TSource, TTick> : TickManager<TSource, TTick>
        where TTick : ScaledTick, new()
    {
        public CustomTickManager(
            Func<TSource, double> extractor,
            MinorTickCollection<TTick> minorTickCollection
        )
            : base(false, extractor, minorTickCollection)
        {
        }

        public override bool GetTickValues( double minValue, double maxValue, out List<TTick> result )
        {
            result = MinorTickCollection.GetAlternatives( minValue, maxValue );
            return true;
        }
    }
}