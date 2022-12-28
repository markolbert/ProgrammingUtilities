// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of MiscellaneousUtilities.
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

using System.Runtime.Versioning;

namespace J4JSoftware.Utilities;

[RequiresPreviewFeatures("Experimental, subject to change or removal")]
public interface ITickRangeConfig
{
}

[RequiresPreviewFeatures("Experimental, subject to change or removal")]
public interface INumericTickRangeConfig : ITickRangeConfig
{
    TickSizePreference TickSizePreference { get; }
}

[RequiresPreviewFeatures("Experimental, subject to change or removal")]
public interface IDateTimeTickRangeConfig : ITickRangeConfig
{
    bool TraditionalMonthsPerMinorOnly { get; }
}