using System;
using System.Linq;

namespace J4JSoftware.DependencyInjection;

public class HasPublicConstructors<T> : ITypeTester
    where T : class
{
    public bool MeetsRequirements(Type toCheck) => toCheck.GetConstructors().Any();
}
