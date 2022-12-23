using System;
using System.IO;
using J4JSoftware.Logging;

namespace J4JSoftware.Utilities;

public static class FileExtensions
{
    public static bool ValidateConfigurationFilePath(
        string path,
        out string? result,
        string? reqdExtension = ".json",
        string? defaultConfigFolder = null,
        IJ4JLogger? logger = null
    )
    {
        result = null;

        var extension = Path.GetExtension( path );
        if( reqdExtension != null && !extension.Equals( reqdExtension, StringComparison.OrdinalIgnoreCase ) )
        {
            path = Path.GetFileNameWithoutExtension( path ) + reqdExtension ?? string.Empty;
            logger?.Warning<string>( "Added required extension ('{0}') to configuration file", reqdExtension );
        }

        // add the default configuration folder path if needed
        defaultConfigFolder ??= Environment.CurrentDirectory;

        var configDir = Path.GetDirectoryName( path );
        if( string.IsNullOrEmpty( configDir ) )
        {
            path = Path.Combine( defaultConfigFolder, path );
            logger?.Information<string>( "Added default configuration folder ('{0}') to configuration file",
                                     defaultConfigFolder );
        }

        //see if the file can be created where specified
        var directory = Path.GetDirectoryName( path )!;
        var testFile = Path.Combine( directory, Guid.NewGuid().ToString() );

        var canCreate = false;

        try
        {
            using var fs = File.Create( testFile );

            fs.Close();
            File.Delete( testFile );

            canCreate = true;
        }
        catch( Exception ex )
        {
            logger?.Warning<string>( "Could not access configuration directory '{0}'", directory );
        }

        if( canCreate )
            result = path;

        return !string.IsNullOrEmpty( result );
    }
}
