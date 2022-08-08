namespace J4JSoftware.DeusEx;

public class J4JDeusExException : Exception
{
    public J4JDeusExException( string msg, Exception? innerException = null )
        : base( msg, innerException )
    {
    }
}