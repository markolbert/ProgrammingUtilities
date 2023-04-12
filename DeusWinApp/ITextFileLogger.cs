#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ITextFileLogger.cs
//
// This file is part of JumpForJoy Software's DeusWinApp.
// 
// DeusWinApp is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// DeusWinApp is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with DeusWinApp. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Runtime.CompilerServices;

namespace J4JSoftware.DeusEx;

public interface ITextFileLogger
{
    void Log(
        string text,
        [ CallerMemberName ] string calledBy = "",
        [ CallerFilePath ] string callerFilePath = "",
        [ CallerLineNumber ] int lineNum = 0
    );

    void Log(
        Exception exception,
        [ CallerMemberName ] string calledBy = "",
        [ CallerFilePath ] string callerFilePath = "",
        [ CallerLineNumber ] int lineNum = 0
    );
}