#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// TypeTesterExtensions.cs
//
// This file is part of JumpForJoy Software's DependencyInjection.
// 
// DependencyInjection is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// DependencyInjection is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with DependencyInjection. If not, see <https://www.gnu.org/licenses/>.
#endregion

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