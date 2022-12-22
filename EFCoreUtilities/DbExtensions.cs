using System;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore;

// ReSharper disable ExplicitCallerInfoArgument

namespace J4JSoftware.QB2LGL;

public static class DbExtensions
{
    public static string FormatDbException(
        this DbUpdateException e,
        [ CallerMemberName ] string callerName = "",
        [ CallerFilePath ] string callerFilePath = "",
        [ CallerLineNumber ] int callerLineNum = 0
    )
    {
        var retVal = new StringBuilder();

        retVal.Append($"\n\tException type:\t{e.GetType().Name}");
        retVal.Append( $"\nDetails:\t{e.Message}" );

        if (e.InnerException != null)
            retVal.Append($"\n\tInner Details:\t{e.InnerException.Message}");

        retVal.Append( "\n\nInvolved entities are:" );

        foreach( var entity in e.Entries )
        {
            retVal.Append( $"\n{entity.Metadata.Name}\n" );

            foreach( var property in entity.Properties )
            {
                retVal.Append( $"\t{property.Metadata.Name}\t{property.CurrentValue}\n" );
            }
        }

        retVal.Append($"\n\n\tCalled by:\t{callerName}");
        retVal.Append($"\n\tSource file:\t{callerFilePath}");
        retVal.Append($"\n\tLine number:\t{callerLineNum}");

        return retVal.ToString();
    }

    public static string FormatException(
        this Exception e,
        string message,
        [ CallerMemberName ] string callerName = "",
        [ CallerFilePath ] string callerFilePath = "",
        [ CallerLineNumber ] int callerLineNum = 0
    )
    {
        var retVal = new StringBuilder( message );

        retVal.Append( $"\n\tException type:\t{e.GetType().Name}" );
        retVal.Append( $"\n\tDetails:\t{e.Message}" );

        if( e.InnerException != null )
            retVal.Append( $"\n\tInner Details:\t{e.InnerException.Message}" );

        retVal.Append( $"\n\n\tCalled by:\t{callerName}" );
        retVal.Append($"\n\tSource file:\t{callerFilePath}");
        retVal.Append($"\n\tLine number:\t{callerLineNum}");

        return retVal.ToString();
    }
}