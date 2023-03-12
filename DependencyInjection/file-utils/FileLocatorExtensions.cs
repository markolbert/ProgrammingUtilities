using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Serilog;

namespace J4JSoftware.DependencyInjection;

[RequiresPreviewFeatures("Under development as of January 2023")]
public static class FileLocatorExtensions
{
    public static FileLocator FileSystemIsCaseSensitive(this FileLocator fileLoc )
    {
        fileLoc.FileSystemIsCaseSensitive = true;
        return fileLoc;
    }

    public static FileLocator FileSystemIsNotCaseSensitive(this FileLocator fileLoc)
    {
        fileLoc.FileSystemIsCaseSensitive = false;
        return fileLoc;
    }

    public static FileLocator FileToFind( this FileLocator fileLoc, string searchPath )
    {
        if( Path.EndsInDirectorySeparator( searchPath ) )
        {
            fileLoc.Logger?.Verbose("File path to find ends in a directory separator character");
            return fileLoc;
        }

        fileLoc.SearchPath = searchPath;

        return fileLoc;
    }

    public static FileLocator StopOnFirstMatch( this FileLocator fileLoc )
    {
        fileLoc.MaximumMatches = 1;
        return fileLoc;
    }

    public static FileLocator FindAllMatches(this FileLocator fileLoc)
    {
        fileLoc.MaximumMatches = -1;
        return fileLoc;
    }

    public static FileLocator FindAtMost( this FileLocator fileLoc, int maxMatches )
    {
        fileLoc.MaximumMatches = maxMatches;
        return fileLoc;
    }

    public static FileLocator Required( this FileLocator fileLoc )
    {
        fileLoc.RequiredPathState |= PathState.Exists;
        return fileLoc;
    }

    public static FileLocator Optional( this FileLocator fileLoc )
    {
        fileLoc.RequiredPathState &= ~PathState.Exists;
        return fileLoc;
    }

    public static FileLocator Readable( this FileLocator fileLoc )
    {
        fileLoc.RequiredPathState |= PathState.Readable;
        return fileLoc;
    }

    public static FileLocator ReadabilityOptional(this FileLocator fileLoc)
    {
        fileLoc.RequiredPathState &= ~PathState.Readable;
        return fileLoc;
    }

    public static FileLocator Writeable( this FileLocator fileLoc )
    {
        fileLoc.RequiredPathState |= PathState.Writeable;
        return fileLoc;
    }

    public static FileLocator WriteabilityOptional(this FileLocator fileLoc)
    {
        fileLoc.RequiredPathState &= ~PathState.Writeable;
        return fileLoc;
    }

    public static FileLocator ScanCurrentDirectory(
        this FileLocator fileLoc,
        IEnumerable<string>? secondaryPaths = null
    ) =>
        fileLoc.MatchesMet ? fileLoc : fileLoc.ScanDirectory( Environment.CurrentDirectory, secondaryPaths );

    public static FileLocator ScanExecutableDirectory(
        this FileLocator fileLoc,
        IEnumerable<string>? secondaryPaths = null
    ) =>
        fileLoc.MatchesMet ? fileLoc : fileLoc.ScanDirectory( AppDomain.CurrentDomain.BaseDirectory, secondaryPaths );

    public static FileLocator ScanDirectories( this FileLocator fileLoc, IEnumerable<string> dirPaths )
    {
        if( fileLoc.MatchesMet )
            return fileLoc;

        foreach( var dirPath in dirPaths )
        {
            fileLoc.ScanDirectory( dirPath );

            if( fileLoc.MatchesMet )
                break;
        }

        return fileLoc;
    }

    public static FileLocator ScanDirectory(
        this FileLocator fileLoc,
        string primaryPath,
        IEnumerable<string>? secondaryPaths = null
    )
    {
        if( fileLoc.MatchesMet )
            return fileLoc;

        if( !EnsurePathExistsAsADirectory(primaryPath, out var temp )  )
            primaryPath = temp;

        if( string.IsNullOrEmpty( primaryPath ) )
        {
            fileLoc.Logger?.Verbose( "No directory specified to search for the file" );
            return fileLoc;
        }

        // start by searching just the primary path
        var primaryResult = SearchForFile( fileLoc, primaryPath );

        if( primaryResult.MeetsRequirements( fileLoc ) )
        {
            fileLoc.Results.Add( primaryResult );

            if( fileLoc.MatchesMet )
                return fileLoc;
        }

        // then search secondary paths, if any
        foreach( var secondaryPath in secondaryPaths ?? Enumerable.Empty<string>() )
        {
            var result = SearchForFile( fileLoc, Path.Combine( primaryPath, secondaryPath ) );

            if( !result.MeetsRequirements( fileLoc ) )
                continue;

            fileLoc.Results.Add( result );

            if( fileLoc.MatchesMet )
                break;
        }

        return fileLoc;
    }

    private static bool EnsurePathExistsAsADirectory( string pathToCheck, out string result )
    {
        result = string.Empty;

        if( string.IsNullOrEmpty( pathToCheck ) )
            return true;

        result = pathToCheck;

        while (!Directory.Exists(result))
        {
            var temp = Path.GetDirectoryName( result );

            if( string.IsNullOrEmpty( temp ) )
                return false;

            result = temp;
        }

        return !string.IsNullOrEmpty( result );
    }

    private static PathInfo SearchForFile( FileLocator fileLoc, string dirPath )
    {
        var retVal = new PathInfo();

        try
        {
            retVal.Path = Path.IsPathRooted(fileLoc.SearchPath)
                ? Path.Combine(dirPath, Path.GetFileName(fileLoc.SearchPath))
                : Path.Combine(dirPath, fileLoc.SearchPath);

            if( File.Exists( retVal.Path ) )
                retVal.State = TestFile( retVal.Path, true, fileLoc.Logger );
            else
            {
                var testFile = Path.Combine( Path.GetDirectoryName( retVal.Path ) ?? string.Empty,
                                             Guid.NewGuid().ToString() );

                retVal.State = TestFile( testFile, false, fileLoc.Logger);
            }
        }
        catch (Exception ex)
        {
            fileLoc.Logger?.Verbose("Problem searching for file, exception message was '{0}'",  ex.Message);
        }

        return retVal;
    }

    private static PathState TestFile( string path, bool fileExists, ILogger? logger )
    {
        var retVal = fileExists ? PathState.Exists : PathState.None;

        if( fileExists )
            retVal |= TestReadability( path, logger ) ? PathState.Readable : PathState.None;

        var writeablePath = fileExists
            ? Path.Combine( Path.GetDirectoryName( path ) ?? string.Empty, Guid.NewGuid().ToString() )
            : path;

        retVal |= TestCanCreateFile( writeablePath, logger) ? PathState.Writeable : PathState.None;

        return retVal;
    }

    private static bool TestReadability(string path, ILogger? logger)
    {
        try
        {
            using var fs = File.OpenRead( path );
            return fs.CanRead;
        }
        catch (Exception ex)
        {
            logger?.Verbose("Could not access file '{0}', exception message was '{1}'",
                                            path,
                                            ex.Message);
        }

        return false;
    }

    private static bool TestCanCreateFile( string path, ILogger? logger )
    {
        try
        {
            using var fs = File.Create( path );
            var retVal = fs.CanWrite;

            fs.Close();
            File.Delete( path );

            return retVal;
        }
        catch (Exception ex)
        {
            logger?.Verbose("Could not access directory '{0}', exception message was '{1}'",
                                            Path.GetDirectoryName(path) ?? string.Empty,
                                            ex.Message);
        }

        return false;
    }
}
