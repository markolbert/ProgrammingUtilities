// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of VisualUtilities.
//
// VisualUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// VisualUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with VisualUtilities. If not, see <https://www.gnu.org/licenses/>.

namespace J4JSoftware.VisualUtilities;

public record CartesianCenter
(
    float CartesianCenterX,
    float CartesianCenterY
);

public record WindowsCartesianContext(
    float CartesianCenterX,
    float CartesianCenterY,
    float WindowsWidth,
    float WindowsHeight
) : CartesianCenter( CartesianCenterX, CartesianCenterY )
{
    public WindowsCartesianContext(
        float windowsWidth,
        float windowsHeight
    )
        : this( 0f, 0f, windowsWidth, windowsHeight )
    {
    }
}
