using System;

namespace J4JSoftware.DependencyInjection;

public class IsAssignableTo<T> : ITypeTester
    where T : class
{
    public bool MeetsRequirements( Type toCheck ) => toCheck.IsAssignableTo( typeof( T ) );
}