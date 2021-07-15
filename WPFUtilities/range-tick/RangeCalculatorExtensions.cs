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
    public static class RangeCalculatorExtensions
    {
        public static RangeParameters? BestByTickCountAndInactiveRegion( 
            this List<RangeParameters> alternatives,
            int targetMajorTicks = 10,
            int targetMinorTicks = 10)
        {
            if( !alternatives.Any() )
                return null;

            targetMajorTicks = targetMajorTicks <= 0 ? 10 : targetMajorTicks;
            targetMinorTicks = targetMinorTicks <= 0 ? 10 : targetMinorTicks;

            return alternatives.OrderBy( x => Math.Abs( targetMajorTicks - (int) x.MajorTicks )
                                              + Math.Abs( targetMinorTicks - (int) x.TickInfo.NumberPerMajor )
                                              + Math.Abs( x.LowerInactiveRegion )
                                              + Math.Abs( x.UpperInactiveRegion ) )
                .First();
        }

        public static RangeParameters? BestByInactiveRegions(
            this List<RangeParameters> alternatives,
            int maxMajorTicks = int.MaxValue
        )
        {
            maxMajorTicks = maxMajorTicks <= 0 ? int.MaxValue : maxMajorTicks;

            return alternatives
                .Where( x => x.MajorTicks <= maxMajorTicks )
                .OrderBy( x => Math.Abs( x.LowerInactiveRegion )
                               + Math.Abs( x.UpperInactiveRegion ) )
                .FirstOrDefault();
        }
    }
}