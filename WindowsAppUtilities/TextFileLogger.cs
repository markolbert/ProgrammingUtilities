using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace J4JSoftware.WindowsAppUtilities;

public class TextFileLogger : ITextFileLogger
{
    private readonly string _logFile;

    public TextFileLogger()
    {
        _logFile = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "bullshit.txt");
    }

    public void Log(
        string text,
        [ CallerMemberName ] string calledBy = "",
        [ CallerFilePath ] string callerFilePath = "",
        [ CallerLineNumber ] int lineNum = 0
    )
    {
        File.AppendAllText( _logFile, $"{calledBy}\t{text}\n\t{callerFilePath}:{lineNum}\n" );
    }

    public void Log(
        Exception e,
        [ CallerMemberName ] string calledBy = "",
        [ CallerFilePath ] string callerFilePath = "",
        [ CallerLineNumber ] int lineNum = 0
    ) =>
        Log( $"Exception {e.GetType().Name} - {e.Message}", calledBy, callerFilePath, lineNum );
}