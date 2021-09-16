﻿#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'AutofacCommandLine' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.DependencyInjection
{
    public abstract class ConstructorTesterBase<T> : ITypeTester
        where T : class
    {
        protected ConstructorTesterBase(
            Type[] reqdParameters,
            bool permuteParameters
        )
        {
            PermuteParameters = permuteParameters;
            RequiredParameters = reqdParameters;

        }

        protected Type[] RequiredParameters { get; private set; }

        public bool PermuteParameters { get; }

        public bool MeetsRequirements( Type toCheck )
        {
            if( !typeof(T).IsAssignableFrom( toCheck ) )
                return false;

            foreach( var ctorParameters in EnumerateParameterList() )
            {
                if( toCheck.GetConstructor( ctorParameters.ToArray() ) != null )
                    return true;
            }

            return false;
        }

        protected abstract IEnumerable<IEnumerable<Type>> EnumerateParameterList();
    }
}