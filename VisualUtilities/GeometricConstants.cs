#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// GeometricConstants.cs
//
// This file is part of JumpForJoy Software's VisualUtilities.
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
#endregion

using System;

namespace J4JSoftware.VisualUtilities;

public static class GeometricConstants
{
    public const float TwoPi = (float)Math.PI * 2;
    public const float HalfPi = (float)Math.PI / 2;
    public const float QuarterPi = (float)Math.PI / 4;
    public const float RadiansPerDegree = (float)Math.PI / 180;
    public const float DegreesPerRadian = (float)(180 / Math.PI);
}
