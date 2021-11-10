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
using System.Linq.Expressions;
using J4JSoftware.Utilities;

namespace J4JSoftware.Utilities
{
    public static class RangeTickExtensions
    {
        public static TickManager<TSource, MonthNumberTick> TickExtractor<TSource>( this List<TSource> tickList,
            Expression<Func<TSource, DateTime>> extractor )
        {
            var temp = extractor.Compile();

            return new CustomTickManager<TSource, MonthNumberTick>( x => MonthNumber.GetMonthNumber( temp( x ) ),
                                                                   new MonthNumberTickCollection() );
        }

        public static TickManager<TSource, ScaledTick> TickExtractor<TSource>( this List<TSource> tickList,
                                                                               Expression<Func<TSource, double>>
                                                                                   extractor )
        {
            var temp = extractor.Compile();

            return new CustomTickManager<TSource, ScaledTick>( x => temp( x ),
                                                              new DoubleTickCollection() );
        }

        public static TickManager<TSource, ScaledTick> TickExtractor<TSource>( this List<TSource> tickList,
                                                                               Expression<Func<TSource, decimal>>
                                                                                   extractor )
        {
            var temp = extractor.Compile();

            return new CustomTickManager<TSource, ScaledTick>( x => (double) temp( x ),
                                                              new DoubleTickCollection() );
        }
    }
}
