using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;

namespace J4JSoftware.DependencyInjection;

public static class TypeTesterExtensions
{
    public static ContainerBuilder RegisterTypeAssemblies<T>(
        this ContainerBuilder builder,
        IEnumerable<ITypeTester> tests
    )
        where T : class
    {
        return builder.RegisterTypeAssemblies<T>(Enumerable.Empty<Assembly>(), false, tests.ToArray());
    }

    public static ContainerBuilder RegisterTypeAssemblies<T>(
        this ContainerBuilder builder,
        bool registerAsSelf,
        IEnumerable<ITypeTester> tests
    )
        where T : class
    {
        return builder.RegisterTypeAssemblies<T>(Enumerable.Empty<Assembly>(), registerAsSelf, tests.ToArray());
    }

    public static ContainerBuilder RegisterTypeAssemblies<T>(
        this ContainerBuilder builder,
        params ITypeTester[] tests
    )
        where T : class
    {
        return builder.RegisterTypeAssemblies<T>(Enumerable.Empty<Assembly>(), false, tests);
    }

    public static ContainerBuilder RegisterTypeAssemblies<T>(
        this ContainerBuilder builder,
        bool registerAsSelf,
        params ITypeTester[] tests
    ) 
        where T : class
    {
        return builder.RegisterTypeAssemblies<T>( Enumerable.Empty<Assembly>(), registerAsSelf, tests );
    }

    public static ContainerBuilder RegisterTypeAssemblies<T>(
        this ContainerBuilder builder,
        IEnumerable<Assembly> assemblies,
        bool registerAsSelf,
        IEnumerable<ITypeTester> tests
    )
        where T : class
    {
        return builder.RegisterTypeAssemblies<T>(assemblies, registerAsSelf, tests.ToArray());
    }


    public static ContainerBuilder RegisterTypeAssemblies<T>(
        this ContainerBuilder builder,
        IEnumerable<Assembly> assemblies,
        bool registerAsSelf,
        params ITypeTester[] tests
    )
        where T : class
    {
        var assemblyList = assemblies.ToList();

        // add default
        assemblyList.Add( typeof( T ).Assembly );

        var temp = builder.RegisterAssemblyTypes( assemblyList.Distinct().ToArray() )
                          .Where( t => tests.All( x => x.MeetsRequirements( t ) ) )
                          .AsImplementedInterfaces();

        if( registerAsSelf )
            temp.AsSelf();

        return builder;
    }

    public static ContainerBuilder RegisterTypeAssemblies<T>(
        this ContainerBuilder builder,
        bool registerAsSelf,
        params PredefinedTypeTests[] predefinedTests
    )
        where T : class
    {
        return builder.RegisterTypeAssemblies<T>( Enumerable.Empty<Assembly>(), registerAsSelf, predefinedTests );
    }

    public static ContainerBuilder RegisterTypeAssemblies<T>(
        this ContainerBuilder builder,
        IEnumerable<Assembly> assemblies,
        bool registerAsSelf,
        params PredefinedTypeTests[] predefinedTests
    )
        where T : class
    {
        var tests = new TypeTests<T>()
           .AddPredefinedTests( predefinedTests );

        return builder.RegisterTypeAssemblies<T>( assemblies, registerAsSelf, tests.ToArray() );
    }
}