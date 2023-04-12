#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// DbExtensions.cs
//
// This file is part of JumpForJoy Software's EFCoreUtilities.
// 
// EFCoreUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// EFCoreUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with EFCoreUtilities. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore;

// ReSharper disable ExplicitCallerInfoArgument

namespace J4JSoftware.EFCoreUtilities;

public static class DbExtensions
{
    public static string FormatDbException(
        this DbUpdateException e,
        [ CallerMemberName ] string callerName = "",
        [ CallerFilePath ] string callerFilePath = "",
        [ CallerLineNumber ] int callerLineNum = 0
    )
    {
        var retVal = new StringBuilder();

        retVal.Append($"\n\tException type:\t{e.GetType().Name}");
        retVal.Append( $"\nDetails:\t{e.Message}" );

        if (e.InnerException != null)
            retVal.Append($"\n\tInner Details:\t{e.InnerException.Message}");

        retVal.Append( "\n\nInvolved entities are:" );

        foreach( var entity in e.Entries )
        {
            retVal.Append( $"\n{entity.Metadata.Name}\n" );

            foreach( var property in entity.Properties )
            {
                retVal.Append( $"\t{property.Metadata.Name}\t{property.CurrentValue}\n" );
            }
        }

        retVal.Append($"\n\n\tCalled by:\t{callerName}");
        retVal.Append($"\n\tSource file:\t{callerFilePath}");
        retVal.Append($"\n\tLine number:\t{callerLineNum}");

        return retVal.ToString();
    }
}