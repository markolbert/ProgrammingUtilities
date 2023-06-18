#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ExpressionExtensions.cs
//
// This file is part of JumpForJoy Software's MiscellaneousUtilities.
// 
// MiscellaneousUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// MiscellaneousUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with MiscellaneousUtilities. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Linq.Expressions;
using System.Reflection;
using System;

namespace J4JSoftware.Utilities;

public static class ExpressionExtensions
{
    public static PropertyInfo GetPropertyInfo<TContainer, TProp>(
        this Expression<Func<TContainer, TProp>> expression
    ) =>
        expression.Body switch
        {
            null => throw new ArgumentNullException( nameof( expression ) ),
            UnaryExpression { Operand: MemberExpression me } => (PropertyInfo) me.Member,
            MemberExpression me => (PropertyInfo) me.Member,
            _ => throw new ArgumentException( $"The expression isn't a valid property. [ {expression} ]" )
        };

    // thanx to https://gist.github.com/jrgcubano/6e4df87913411ee9db0c68efc5fc41a3
    // for these next two methods
    public static Action<TEntity, TProperty> CreateSetter<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> property)
    {
        var propertyInfo = property.GetPropertyInfo();

        var setMethod = propertyInfo.GetSetMethod()
         ?? throw new ArgumentNullException( $"Property {propertyInfo.Name} does not have a public setter" );

        var instance = Expression.Parameter(typeof(TEntity), "instance");
        var parameter = Expression.Parameter(typeof(TProperty), "param");

        var body = Expression.Call(instance, setMethod, parameter);
        var parameters = new[] { instance, parameter };

        return Expression.Lambda<Action<TEntity, TProperty>>(body, parameters).Compile();
    }

    public static Func<TEntity, TProperty> CreateGetter<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> property)
    {
        var propertyInfo = property.GetPropertyInfo();

        var getMethod = propertyInfo.GetGetMethod()
         ?? throw new ArgumentNullException($"Property {propertyInfo.Name} does not have a public getter");

        var instance = Expression.Parameter(typeof(TEntity), "instance");

        var body = Expression.Call(instance, getMethod);
        var parameters = new[] { instance };

        return Expression.Lambda<Func<TEntity, TProperty>>(body, parameters).Compile();
    }
}