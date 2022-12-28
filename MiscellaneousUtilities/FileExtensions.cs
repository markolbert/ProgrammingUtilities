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

using System;
using System.IO;
using J4JSoftware.Logging;
using Serilog.Core;

namespace J4JSoftware.Utilities;

public static class FileExtensions
{
    public static bool ValidateFilePath(
        string path,
        out string? result,
        string? reqdExtension = ".json",
        string? defaultFolder = null,
        bool requireWriteAccess = false,
        IJ4JLogger? logger = null
    )
    {
        var extension = Path.GetExtension( path );
        if( reqdExtension != null && !extension.Equals( reqdExtension, StringComparison.OrdinalIgnoreCase ) )
        {
            path = Path.GetFileNameWithoutExtension( path ) + reqdExtension;
            logger?.Warning<string>( "Added required extension ('{0}') to file", reqdExtension );
        }

        // add the default target folder path if needed
        defaultFolder ??= Environment.CurrentDirectory;

        var fileDirectory = Path.GetDirectoryName( path );
        if( string.IsNullOrEmpty( fileDirectory ) )
        {
            path = Path.Combine( defaultFolder, path );
            logger?.Information<string>( "Added default folder ('{0}') to file", defaultFolder );
        }

        // if only read access is required, confirm the file exists and is accessible
        return requireWriteAccess
            ? CheckFileCanBeCreated( path, out result, logger )
            : CheckFileExists( path, out result, logger );
    }

    private static bool CheckFileExists( string path, out string? result, IJ4JLogger? logger )
    {
        result = null;

        bool fileExists;

        try
        {
            fileExists = File.Exists(path);
        }
        catch
        {
            logger?.Warning("File '{0}' is not accessible");
            return false;
        }

        if (fileExists)
        {
            result = path;
            return true;
        }

        logger?.Warning("File '{0}' does not exist");
        return false;
    }

    private static bool CheckFileCanBeCreated( string path, out string? result, IJ4JLogger? logger )
    {
        //see if the file can be created where specified
        var directory = Path.GetDirectoryName(path)!;
        var testFile = Path.Combine(directory, Guid.NewGuid().ToString());

        var canCreate = false;

        try
        {
            using var fs = File.Create(testFile);

            fs.Close();
            File.Delete(testFile);

            canCreate = true;
        }
        catch
        {
            logger?.Warning<string>("Could not access directory '{0}'", directory);
        }

        result = canCreate ? path : null;

        return !string.IsNullOrEmpty(result);
    }
}
