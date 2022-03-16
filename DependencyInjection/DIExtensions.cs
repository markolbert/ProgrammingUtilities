using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;

namespace J4JSoftware.DependencyInjection
{
    public static class DIExtensions
    {
        public static ContainerBuilder RegisterTypeAssemblies<T>(
            this ContainerBuilder builder,
            bool registerAsSelf = false,
            params ITypeTester[] tests
        ) 
            where T : class
            =>
            builder.RegisterTypeAssemblies<T>( Enumerable.Empty<Assembly>(), tests, registerAsSelf);

        public static ContainerBuilder RegisterTypeAssemblies<T>(
            this ContainerBuilder builder,
            IEnumerable<ITypeTester> tests,

            bool registerAsSelf = false
        )
            where T : class =>
            builder.RegisterTypeAssemblies<T>( Enumerable.Empty<Assembly>(), tests, registerAsSelf );

        public static ContainerBuilder RegisterTypeAssemblies<T>( this ContainerBuilder builder,
                                                                  IEnumerable<Assembly> assemblies,
                                                                  IEnumerable<ITypeTester> tests,
                                                                  bool registerAsSelf = false )
            where T : class
        {
            var assemblyList = assemblies.ToList();

            // add default
            assemblyList.Add( typeof( T ).Assembly );

            var temp = builder.RegisterAssemblyTypes( assemblyList.Distinct().ToArray() )
                              .Where( t => tests.All( x =>
                                                      {
                                                          var retVal = x.MeetsRequirements( t );
                                                          return retVal;
                                                      } ) )
                              .AsImplementedInterfaces();

            if ( registerAsSelf )
                temp.AsSelf();

            return builder;
        }

        public static ContainerBuilder RegisterTypeAssemblies<T>(
            this ContainerBuilder builder,
            bool registerAsSelf = false,
            params PredefinedTypeTests[] predefinedTests
        )
            where T : class =>
            builder.RegisterTypeAssemblies<T>( Enumerable.Empty<Assembly>(), predefinedTests, registerAsSelf);

        public static ContainerBuilder RegisterTypeAssemblies<T>(
            this ContainerBuilder builder,
            IEnumerable<Assembly> assemblies,
            IEnumerable<PredefinedTypeTests> predefinedTests,

            bool registerAsSelf = false
        )
            where T : class
        {
            var tests = new TypeTests<T>()
                .AddTests( predefinedTests );

            return builder.RegisterTypeAssemblies<T>( assemblies, tests, registerAsSelf );
        }
    }
}
