using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using J4JSoftware.Logging;

namespace J4JSoftware.DependencyInjection
{
    public static class DIExtensions
    {
        public static ContainerBuilder RegisterTypeAssemblies<T>( this ContainerBuilder builder,
                                                                  IEnumerable<Assembly> assemblies,
                                                                  bool registerAsSelf = false,
                                                                  params ITypeTester[] tests )
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

        public static ContainerBuilder RegisterTypeAssemblies<T>( this ContainerBuilder builder,
                                                                  IEnumerable<Assembly> assemblies,
                                                                  bool registerAsSelf = false,
                                                                  params PredefinedTypeTests[] typeTests )
            where T : class
        {
            var assemblyList = assemblies.ToList();

            // add default
            assemblyList.Add( typeof( T ).Assembly );

            var tests = new List<ITypeTester>();

            foreach ( var test in typeTests.Distinct() )
            {
                switch ( test )
                {
                    case PredefinedTypeTests.ParameterlessConstructor:
                        tests.Add( new ConstructorTester<T>() );
                        break;

                    case PredefinedTypeTests.OnlyJ4JLoggerRequired:
                        tests.Add( new ConstructorTester<T>( typeof( IJ4JLogger ) ) );
                        break;

                    //case PredefinedTypeTests.OnlyJ4JLoggerFactoryRequired:
                    //    tests.Add(new ConstructorTester<T>(typeof(IJ4JLoggerFactory)));
                    //    break;

                    case PredefinedTypeTests.NonAbstract:
                        tests.Add( TypeTester.NonAbstract );
                        break;

                    default:
                        throw new
                            InvalidEnumArgumentException( $"Unsupported {nameof( PredefinedTypeTests )} value '{test}'" );
                }
            }

            var temp = builder.RegisterAssemblyTypes( assemblyList.Distinct().ToArray() )
                              .Where( t => tests.All( x => x.MeetsRequirements( t ) ) )
                              .AsImplementedInterfaces();

            if ( registerAsSelf )
                temp.AsSelf();

            return builder;
        }
    }
}
