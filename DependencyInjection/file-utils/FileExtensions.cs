#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// FileExtensions.cs
//
// This file is part of JumpForJoy Software's DependencyInjection.
// 
// DependencyInjection is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// DependencyInjection is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with DependencyInjection. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;

namespace J4JSoftware.DependencyInjection;

public static class FileExtensions
{
    public static bool ValidateFilePath(
        string path,
        out string? result,
        string? reqdExtension = ".json",
        IEnumerable<string>? folders = null,
        bool requireWriteAccess = false,
        ILogger? logger = null
    )
    {
        logger?.Verbose("Validating file path '{0}'", path);

        folders ??= Enumerable.Empty<string>();

        var extension = Path.GetExtension( path );
        if( reqdExtension != null && !extension.Equals( reqdExtension, StringComparison.OrdinalIgnoreCase ) )
        {
            path = Path.GetFileNameWithoutExtension( path ) + reqdExtension;
            logger?.Verbose( "Added required extension ('{0}') to file", reqdExtension );
        }

        var fileOkay = requireWriteAccess
            ? CheckFileCanBeCreated(path, out result, logger)
            : CheckFileExists(path, out result, logger);

        if( fileOkay )
        {
            logger?.Verbose( "Found file {0}", result! );
            return true;
        }

        // we didn't find the file based just on its current path, so
        // look for it in the folders we were given. How we do this
        // depends on whether the path we were given is rooted.
        var folderList = folders.ToList();

        var directoryPath = Path.GetDirectoryName( path );
        if( !Path.IsPathRooted( directoryPath ) )
        {
            if( CheckAlternativeLocations( path,
                                           folderList,
                                           requireWriteAccess,
                                           out result,
                                           logger ) )
            {
                logger?.Verbose("Found file {0}", result!);
                return true;
            }
        }

        path = Path.GetFileName( path );

        if( CheckAlternativeLocations( Path.GetFileName( path ), folderList, requireWriteAccess, out result, logger ) )
        {
            logger?.Verbose("Found file {0}", result!);
            return true;
        }

        logger?.Information("Could not find '{0}' in any of the supplied folders", path);

        return false;
    }

    private static bool CheckAlternativeLocations(
        string path,
        List<string> folderList,
        bool requireWriteAccess,
        out string? result,
        ILogger? logger
    )
    {
        result = null;
        string? pathToCheck = null;

        foreach( var folder in folderList )
        {
            logger?.Verbose( "Checking folder '{0}' for {1}", folder, path );

            try
            {
                pathToCheck = Path.Combine( folder, path );

                if( requireWriteAccess
                       ? CheckFileCanBeCreated( pathToCheck, out result, logger )
                       : CheckFileExists( pathToCheck, out result, logger ) )
                    return true;
            }
            catch( Exception ex )
            {
                logger?.Error("Exception when trying to access '{0}', message was '{1}'",
                    pathToCheck!,
                    ex.Message);
                return false;
            }
        }

        return false;
    }

    private static bool CheckFileExists( string path, out string? result, ILogger? logger )
    {
        result = null;

        bool fileExists;

        try
        {
            fileExists = File.Exists(path);
        }
        catch
        {
            logger?.Verbose("File '{0}' is not accessible", path);
            return false;
        }

        if (fileExists)
        {
            result = path;
            return true;
        }

        logger?.Verbose("File '{0}' does not exist", path);
        return false;
    }

    private static bool CheckFileCanBeCreated( string path, out string? result, ILogger? logger )
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
            logger?.Verbose("Could not access directory '{0}'", directory);
        }

        result = canCreate ? path : null;

        return !string.IsNullOrEmpty(result);
    }
}
