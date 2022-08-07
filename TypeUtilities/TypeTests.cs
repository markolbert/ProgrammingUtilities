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
    private readonly List<ITypeTester> _tests = new();

    public TypeTests<T> AddTests( params PredefinedTypeTests[] predefined ) =>
        AddTests( predefined.AsEnumerable() );

    public TypeTests<T> AddTests( IEnumerable<PredefinedTypeTests> predefined )
    {
        foreach( var curItem in predefined )
        {
            switch (curItem)
            {
                case PredefinedTypeTests.ParameterlessConstructor:
                    _tests.Add(new ConstructorParameterTester<T>());
                    break;

                case PredefinedTypeTests.OnlyJ4JLoggerRequired:
                    _tests.Add(new ConstructorParameterTester<T>(typeof(IJ4JLogger)));
                    break;

                case PredefinedTypeTests.NonAbstract:
                    _tests.Add(TypeTester.NonAbstract);
                    break;

                case PredefinedTypeTests.NonGeneric:
                    _tests.Add(TypeTester.NonGeneric);
                    break;

                default:
                    throw new
                        InvalidEnumArgumentException($"Unsupported {nameof(PredefinedTypeTests)} value '{curItem}'");
            }
        }

        return this;
    }

    public TypeTests<T> HasRequiredConstructor( params Type[] ctorParameters ) =>
        HasRequiredConstructor( ctorParameters.AsEnumerable() );

    public TypeTests<T> HasRequiredConstructor( IEnumerable<Type> ctorParameters )
    {
        _tests.Add(new ConstructorParameterTester<T>(ctorParameters)  );

        return this;
    }

    public TypeTests<T> AddTests( params ITypeTester[] tests ) => AddTests( tests.AsEnumerable() );

    public TypeTests<T> AddTests( IEnumerable<ITypeTester> tests )
    {
        _tests.AddRange(tests);

        return this;
    }

    public IEnumerator<ITypeTester> GetEnumerator()
    {
        foreach( var test in _tests )
        {
            yield return test;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
