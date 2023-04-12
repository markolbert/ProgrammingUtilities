#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// TypeTestExtensions.cs
//
// This file is part of JumpForJoy Software's TypeUtilities.
// 
// TypeUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// TypeUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with TypeUtilities. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.DependencyInjection;

public static class TypeTestExtensions
{
    #region Add tests methods

    public static TypeTests<T> AddPredefinedTests<T>( this TypeTests<T> tester, params PredefinedTypeTests[] predefined )
        where T : class =>
        tester.AddPredefinedTests( predefined.AsEnumerable() );

    public static TypeTests<T> AddPredefinedTests<T>( this TypeTests<T> tester, IEnumerable<PredefinedTypeTests> predefined )
        where T : class
    {
        foreach( var curItem in predefined )
        {
            switch( curItem )
            {
                case PredefinedTypeTests.ParameterlessConstructor:
                    tester.Tests.Add( new ConstructorParameterTester<T>() );
                    break;

                case PredefinedTypeTests.OnlyJ4JLoggerRequired:
                    tester.Tests.Add( new ConstructorParameterTester<T>( typeof( ILogger ) ) );
                    break;

                case PredefinedTypeTests.NonAbstract:
                    tester.Tests.Add( TypeTester.NonAbstract );
                    break;

                case PredefinedTypeTests.NonGeneric:
                    tester.Tests.Add( TypeTester.NonGeneric );
                    break;

                default:
                    throw new
                        InvalidEnumArgumentException(
                            $"Unsupported {nameof( PredefinedTypeTests )} value '{curItem}'" );
            }
        }

        return tester;
    }

    public static TypeTests<T> HasConstructorArgs<T>( this TypeTests<T> tester, params Type[] ctorParameters )
        where T : class =>
        tester.HasConstructorArgs( ctorParameters.AsEnumerable() );

    public static TypeTests<T> HasConstructorArgs<T>( this TypeTests<T> tester, IEnumerable<Type> ctorParameters )
        where T : class
    {
        tester.Tests.Add( new ConstructorParameterTester<T>( ctorParameters ) );

        return tester;
    }

    public static TypeTests<T> DecoratedWith<T>(
        this TypeTests<T> tester,
        Type requiredAttribute,
        bool allowInherited = false
    )
        where T : class
    {
        tester.Tests.Add( new DecoratedTypeTester<T>( allowInherited, requiredAttribute ) );

        return tester;
    }

    public static TypeTests<T> FilteredBy<T>(
        this TypeTests<T> tester,
        Func<Type, bool> filter
    )
        where T : class
    {
        tester.Tests.Add(new FilterTypeTester<T>(filter)  );

        return tester;
    }

    public static TypeTests<T> AddTests<T>( this TypeTests<T> tester, params ITypeTester[] tests )
        where T : class
    {
        tester.Tests.AddRange(tests);

        return tester;
    }

    #endregion

    #region MeetsRequirements methods

    public static IEnumerable<Type> MeetRequirements<T>(
        this IEnumerable<Type> types,
        params ITypeTester[] tests
    )
        where T : class =>
        types.Where( t => typeof( T ).IsAssignableFrom( t )
                      && tests.All( x => x.MeetsRequirements( t ) ) );

    public static IEnumerable<Type> MeetRequirements<T>(
        this IEnumerable<Type> types,
        IEnumerable<ITypeTester> tests
    )
        where T : class =>
        types.Where( t => typeof( T ).IsAssignableFrom( t )
                      && tests.All( x => x.MeetsRequirements( t ) ) );

    public static IEnumerable<Type> MeetRequirements<T>(
        this IEnumerable<Type> types,
        params PredefinedTypeTests[] predefinedTests
    )
        where T : class =>
        types.MeetRequirements<T>( predefinedTests.ToTypeTesters<T>() );

    public static IEnumerable<Type> MeetRequirements<T>(
        this IEnumerable<Type> types,
        IEnumerable<PredefinedTypeTests> predefinedTests
    )
        where T : class =>
        types.MeetRequirements<T>( predefinedTests.ToTypeTesters<T>() );

    #endregion

    private static List<ITypeTester> ToTypeTesters<T>( this IEnumerable<PredefinedTypeTests> predefinedTests )
        where T : class
    {
        var retVal = new List<ITypeTester>();

        foreach( var test in predefinedTests.Distinct() )
        {
            switch( test )
            {
                case PredefinedTypeTests.ParameterlessConstructor:
                    retVal.Add( new ConstructorParameterTester<T>() );
                    break;

                case PredefinedTypeTests.OnlyJ4JLoggerRequired:
                    retVal.Add( new ConstructorParameterTester<T>( typeof( ILogger ) ) );
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