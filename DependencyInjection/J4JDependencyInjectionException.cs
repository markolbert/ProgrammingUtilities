using System;

namespace J4JSoftware.DependencyInjection;

public class J4JDependencyInjectionException : Exception
{
    public J4JDependencyInjectionException(
        string message
    )
        : base( message )
    {
    }

    public J4JDependencyInjectionException(
        string message,
        Exception inner
    )
        : base( message, inner )
    {

    }

    public J4JDependencyInjectionException(
        string message,
        Exception inner,
        J4JHostConfiguration hostConfig
    )
        : base( message, inner )
    {
        HostConfiguration = hostConfig;
    }

    public J4JDependencyInjectionException(
        string message,
        J4JHostConfiguration hostConfig
    )
        : base( message )
    {
        HostConfiguration = hostConfig;
    }

    public J4JHostConfiguration? HostConfiguration { get; }
}
