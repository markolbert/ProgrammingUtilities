using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.DependencyInjection;

public class FilterTypeTester<T> : ConstructorTesterBase<T>
    where T : class
{
    private readonly Func<Type, bool> _filter;

    public FilterTypeTester(
        Func<Type, bool> filter
        )
    {
        _filter = filter;
    }

    public override bool MeetsRequirements( Type toCheck ) => base.MeetsRequirements( toCheck ) && _filter( toCheck );
}
