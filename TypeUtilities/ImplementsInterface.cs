using System;

namespace J4JSoftware.DependencyInjection;

public class ImplementsInterface<T> : ITypeTester
    where T : class
{
    public bool MeetsRequirements( Type toCheck ) =>
        typeof(T).GetInterface( toCheck.FullName ?? string.Empty ) != null;
}
