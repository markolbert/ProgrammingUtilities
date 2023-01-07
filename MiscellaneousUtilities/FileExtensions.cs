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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using J4JSoftware.Logging;
using Serilog.Core;

namespace J4JSoftware.Utilities;

public static class FileExtensions
{
    public static bool ValidateFilePath(
        string path,
        out string? result,
        string? reqdExtension = ".json",
        IEnumerable<string>? folders = null,
        bool requireWriteAccess = false,
        IJ4JLogger? logger = null
    )
    {
        logger?.Verbose<string>("Validating file path '{0}'", path);

        folders ??= Enumerable.Empty<string>();

        var extension = Path.GetExtension( path );
        if( reqdExtension != null && !extension.Equals( reqdExtension, StringComparison.OrdinalIgnoreCase ) )
        {
            path = Path.GetFileNameWithoutExtension( path ) + reqdExtension;
            logger?.Verbose<string>( "Added required extension ('{0}') to file", reqdExtension );
        }

        var fileOkay = requireWriteAccess
            ? CheckFileCanBeCreated(path, out result, logger)
            : CheckFileExists(path, out result, logger);

        if( fileOkay )
            return true;

        // we didn't find the file based just on its current path, so, if it didn't
        // have an explicit directory associated with it, look for it in the folders
        // we were given.
        if( !string.IsNullOrEmpty( Path.GetDirectoryName( path ) ) )
        {
            logger?.Information<string>( "File '{0}' has a directory path but was not found", path );
            return false;
        }

        string? pathToCheck = null;

        foreach ( var folder in folders )
        {
            logger?.Verbose<string>( "Checking folder '{0}' for the file", folder );

            try
            {
                pathToCheck = Path.Combine( folder, path );

                if( requireWriteAccess
                       ? CheckFileCanBeCreated( pathToCheck, out result, logger )
                       : CheckFileExists( pathToCheck, out result, logger ) )
                    return true;
            }
            catch ( Exception ex )
            {
                logger?.Error<string, string>( "Exception when trying to access '{0}', message was '{1}'",
                                               pathToCheck!,
                                               ex.Message );
                return false;
            }
        }

        logger?.Information<string>("Could not find '{0}' in any of the supplied folders", path);

        return false;
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
            logger?.Verbose<string>("File '{0}' is not accessible", path);
            return false;
        }

        if (fileExists)
        {
            result = path;
            return true;
        }

        logger?.Verbose<string>("File '{0}' does not exist", path);
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
            logger?.Verbose<string>("Could not access directory '{0}'", directory);
        }

        result = canCreate ? path : null;

        return !string.IsNullOrEmpty(result);
    }
}
