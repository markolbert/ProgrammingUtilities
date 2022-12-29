using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.Utilities;

public static class ExceptionExtensions
{
    public static string FormatException(
        this Exception e,
        string message,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNum = 0
    )
    {
        var retVal = new StringBuilder(message);

        retVal.Append($"\n\tException type:\t{e.GetType().Name}");
        retVal.Append($"\n\tDetails:\t{e.Message}");

        if (e.InnerException != null)
            retVal.Append($"\n\tInner Details:\t{e.InnerException.Message}");

        retVal.Append($"\n\n\tCalled by:\t{callerName}");
        retVal.Append($"\n\tSource file:\t{callerFilePath}");
        retVal.Append($"\n\tLine number:\t{callerLineNum}");

        return retVal.ToString();
    }
}