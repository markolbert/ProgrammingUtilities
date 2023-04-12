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
            UnaryExpression ue when ue.Operand is MemberExpression me => (PropertyInfo) me.Member,
            MemberExpression me => (PropertyInfo) me.Member,
            _ => throw new ArgumentException( $"The expression isn't a valid property. [ {expression} ]" )
        };
}