using System;

namespace J4JSoftware.DependencyInjection;

public class IsAssignableFrom<T> : ITypeTester
    where T : class
{
    public bool MeetsRequirements(Type toCheck) => typeof(T).IsAssignableFrom(toCheck);
}