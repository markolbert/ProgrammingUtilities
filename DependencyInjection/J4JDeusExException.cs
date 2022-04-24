using System;

namespace J4JSoftware.DependencyInjection;

public class J4JDeusExException : Exception
{
    public J4JDeusExException( string msg, Exception? innerException = null )
        : base( msg, innerException )
    {
    }
}
