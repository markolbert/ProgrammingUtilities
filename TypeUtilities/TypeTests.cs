using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.DependencyInjection;

public class TypeTests<T> : IEnumerable<ITypeTester>
    where T : class
{
    public Type TypeToTest { get; } = typeof(T);

    internal List<ITypeTester> Tests { get; } = new();

    public IEnumerator<ITypeTester> GetEnumerator()
    {
        // always start by checking if the type being tested is assignable from type T
        yield return new IsAssignableFrom<T>();

        yield return new HasPublicConstructors<T>();

        foreach( var test in Tests )
        {
            yield return test;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
