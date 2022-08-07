using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.DependencyInjection
{
    public static class TypeTestExtensions
    {
        public static IEnumerable<Type> MeetRequirements<T>( this IEnumerable<Type> types,
                                                             params ITypeTester[] tests )
            where T : class =>
            types.Where( t => typeof( T ).IsAssignableFrom( t )
                              && tests.All( x => x.MeetsRequirements( t ) ) );

        public static IEnumerable<Type> MeetRequirements<T>( this IEnumerable<Type> types,
                                                             IEnumerable<ITypeTester> tests )
            where T : class =>
            types.Where( t => typeof( T ).IsAssignableFrom( t )
                              && tests.All( x => x.MeetsRequirements( t ) ) );

        public static IEnumerable<Type> MeetRequirements<T>( this IEnumerable<Type> types,
                                                             params PredefinedTypeTests[] predefinedTests )
            where T : class =>
            types.MeetRequirements<T>( predefinedTests.ToTypeTesters<T>() );

        public static IEnumerable<Type> MeetRequirements<T>( this IEnumerable<Type> types,
                                                             IEnumerable<PredefinedTypeTests> predefinedTests )
            where T : class =>
            types.MeetRequirements<T>( predefinedTests.ToTypeTesters<T>() );

        private static List<ITypeTester> ToTypeTesters<T>( this IEnumerable<PredefinedTypeTests> predefinedTests )
            where T : class
        {
            var retVal = new List<ITypeTester>();

            foreach ( var test in predefinedTests.Distinct() )
            {
                switch ( test )
                {
                    case PredefinedTypeTests.ParameterlessConstructor:
                        retVal.Add( new ConstructorParameterTester<T>() );
                        break;

                    case PredefinedTypeTests.OnlyJ4JLoggerRequired:
                        retVal.Add( new ConstructorParameterTester<T>( typeof( IJ4JLogger ) ) );
                        break;

                    case PredefinedTypeTests.NonAbstract:
                        retVal.Add( TypeTester.NonAbstract );
                        break;

                    case PredefinedTypeTests.NonGeneric:
                        retVal.Add( TypeTester.NonGeneric );
                        break;

                    default:
                        throw new
                            InvalidEnumArgumentException( $"Unsupported {nameof( PredefinedTypeTests )} value '{test}'" );
                }
            }

            return retVal;
        }
    }
}
