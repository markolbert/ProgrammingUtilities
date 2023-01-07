using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using J4JSoftware.Logging;

namespace J4JSoftware.Utilities;

public class FileFolders : IEnumerable<string>
{
    public static FileFolders Default { get; } = new();

    private readonly List<string> _folders = new();

    public FileFolders(
        bool inclCurDir = true,
        bool inclEntryDir = true
    )
    {
        if( inclCurDir )
            _folders.Add( Environment.CurrentDirectory );

        if( !inclEntryDir )
            return;

        _folders.Add( AppDomain.CurrentDomain.BaseDirectory );
    }

    public void Add( string folder )
    {
        _folders.Add( folder );
    }

    public IEnumerator<string> GetEnumerator()
    {
        foreach( var folder in _folders )
        {
            yield return folder;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
