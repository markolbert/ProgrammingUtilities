﻿#region license

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

namespace J4JSoftware.Utilities
{
    public record NumericTick
    {
        public NumericTick(
            uint ticksPer10
        )
        {
            if (ticksPer10 == 0 || ticksPer10 > 10 )
                throw new ArgumentException($"{nameof(ticksPer10)} must be a positive number <= 10");

            TicksPer10 = ticksPer10;
            NormalizedSize = 1M / ticksPer10;

            while( NormalizedSize < 1 )
            {
                NormalizedSize *= 10;
            }
        }

        public uint TicksPer10 { get; }
        public decimal NormalizedSize { get; }
    }
}