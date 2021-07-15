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
using System.Linq;

namespace J4JSoftware.WPFUtilities
{
    public record TickManager<TSource, TTick> : ITickManager<TSource, TTick>
        where TTick : ScaledTick, new()
    {
        protected TickManager(
            bool isSimpleExtractor,
            Func<TSource, double> extractor,
            MinorTickCollection<TTick> minorTickCollection
        )
        {
            IsSimpleExtractor = isSimpleExtractor;
            Extractor = extractor;

            MinorTickCollection = minorTickCollection;
        }

        public bool IsSimpleExtractor { get; }

        protected Func<TSource, double> Extractor { get; init; }
        protected MinorTickCollection<TTick> MinorTickCollection { get; }

        public Type SourceType => typeof(TSource);
        public Type TickType => typeof(TTick);

        public bool ExtractDouble(TSource source, out double result)
        {
            result = Extractor(source);
            return true;
        }

        public bool ExtractDoubles( IEnumerable<TSource> source, out List<double>? result )
        {
            result = source.Select( x => Extractor( x ) ).ToList();
            return true;
        }

        public bool GetTickValues(double minValue, double maxValue, out List<TTick> result)
        {
            result = MinorTickCollection.GetAlternatives( minValue, maxValue );
            return true;
        }

        public RangeParameters GetDefaultRange(double minValue, double maxValue) =>
            MinorTickCollection.GetDefaultRangeParameters(minValue, maxValue);

        bool ITickManager.ExtractDouble(object source, out double? result)
        {
            result = null;

            if( source is not TSource castSource ) 
                return false;

            result = Extractor(castSource);
            
            return true;
        }

        bool ITickManager.ExtractDoubles( IEnumerable<object> source, out List<double>? result )
        {
            result = null;

            try
            {
                result = source.Select( x => Extractor( (TSource) x ) ).ToList();
            }
            catch
            {
                return false;
            }

            return true;
        }

        List<ScaledTick> ITickManager.GetTickValues( double minValue, double maxValue ) =>
            MinorTickCollection.GetAlternatives( minValue, maxValue ).Cast<ScaledTick>().ToList();
    }
}