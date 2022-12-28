// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of DeusEx.
//
// DeusEx is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// DeusEx is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with DeusEx. If not, see <https://www.gnu.org/licenses/>.

namespace J4JSoftware.DeusEx;

public class J4JDeusExException : Exception
{
    public J4JDeusExException( string msg, Exception? innerException = null )
        : base( msg, innerException )
    {
    }
}