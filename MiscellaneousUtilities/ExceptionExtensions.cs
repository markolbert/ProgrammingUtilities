﻿#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// 
// This file is part of J4JLogger.
//
// J4JLogger is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JLogger is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JLogger. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace J4JSoftware.Utilities;

public static class ExceptionExtensions
{
    public static string FormatException(
        this Exception e,
        string message,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNum = 0
    )
    {
        var retVal = new StringBuilder(message);

        retVal.Append($"\n\tException type:\t{e.GetType().Name}");
        retVal.Append($"\n\tDetails:\t{e.Message}");

        if (e.InnerException != null)
            retVal.Append($"\n\tInner Details:\t{e.InnerException.Message}");

        retVal.Append($"\n\n\tCalled by:\t{callerName}");
        retVal.Append($"\n\tSource file:\t{callerFilePath}");
        retVal.Append($"\n\tLine number:\t{callerLineNum}");

        return retVal.ToString();
    }
}