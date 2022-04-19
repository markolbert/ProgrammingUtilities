using System;
using System.Runtime.CompilerServices;

namespace J4JSoftware.WindowsAppUtilities
{
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

}
