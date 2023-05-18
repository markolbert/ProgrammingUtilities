using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using J4JSoftware.DependencyInjection;
using Xunit;

namespace Test.DependencyInjection;

public class FileLocatorTests
{
    [ Theory ]
    [ InlineData( "test.json", 1 ) ]
    [ InlineData( "test2.json", 0 ) ]
    [ InlineData( "test.txt", 0 ) ]
    [ InlineData( "fileloc/test.json", 1 ) ]
    [ InlineData( "fileloc/test2.json", 0 ) ]
    [ InlineData( "fileloc/test.txt", 0 ) ]
    public void FileExistsInExeDir( string path, int matches )
    {
        var fileLoc = new FileLocator()
                     .FileToFind( path )
                     .Required()
                     .ScanExecutableDirectory();

        fileLoc.Matches.Should().Be( matches );
    }

    [ Theory ]
    [ InlineData( "test.json", 1 ) ]
    [ InlineData( "test2.json", 0 ) ]
    [ InlineData( "test.txt", 0 ) ]
    [ InlineData( "fileloc/test.json", 1 ) ]
    [ InlineData( "fileloc/test2.json", 0 ) ]
    [ InlineData( "fileloc/test.txt", 0 ) ]
    public void FileExistsInCurrentDir( string path, int matches )
    {
        var fileLoc = new FileLocator()
                     .FileToFind( path )
                     .Required()
                     .ScanCurrentDirectory();

        fileLoc.Matches.Should().Be( matches );
    }

    [ Theory ]
    [ InlineData( "test.json", 1 ) ]
    [ InlineData( "test2.json",0 ) ]
    public void DirectoryIsWriteable( string path, int matches )
    {
        var fileLoc = new FileLocator()
                     .FileToFind( path )
                     .Required();

        fileLoc.ScanExecutableDirectory();

        fileLoc.Matches.Should().Be( matches );
    }

    [ Theory ]
    [ InlineData( "test3.json", 1, "test3.json", "fileLoc", "" ) ]
    [ InlineData( "test4.json", 1, "fileloc\\test4.json", "fileLoc", "" ) ]
    public void ScanDirectories( string fileToFind, int matches, string foundPath, params string[] dirToSearch )
    {
        var toSearch = dirToSearch.Select( x => Path.Combine( Environment.CurrentDirectory, x ) );
        foundPath = Path.Combine( Environment.CurrentDirectory, foundPath );

        var fileLoc = new FileLocator()
                     .FileToFind( fileToFind )
                     .Required()
                     .ScanDirectories( toSearch );

        fileLoc.Matches.Should().Be( matches );
        fileLoc.FirstMatch.Should().NotBeNull();
        fileLoc.FirstMatch!.Path.ToLower().Should().Be( foundPath.ToLower() );
    }
}
