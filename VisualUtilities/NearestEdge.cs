#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// NearestEdge.cs
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

[Flags]
public enum NearestEdge
{
    Top = 1 << 0,
    Right = 1 << 1,
    Bottom = 1 << 2,
    Left = 1 << 3,

    Internal = 0
}
