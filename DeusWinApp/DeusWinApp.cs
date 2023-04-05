#region copyright
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

using System.IO;
using J4JSoftware.DependencyInjection;

namespace J4JSoftware.DeusEx;

public abstract class J4JDeusExWinApp : J4JDeusExHosted
{
    protected override string GetCrashFilePath( J4JHostConfiguration hostConfig, string crashFileName = "crashFile.txt" )
    {
        var fileName = Path.GetFileName(crashFileName);

        if (string.IsNullOrEmpty(fileName))
            fileName = "crashFile.txt";

        return Path.Combine( Windows.Storage.ApplicationData.Current.LocalFolder.Path, fileName );
    }
}