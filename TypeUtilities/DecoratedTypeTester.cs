using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.DependencyInjection;

public class DecoratedTypeTester<T> : ConstructorTesterBase<T>
    where T : class
{
    private readonly bool _allowInherited;
    private readonly List<Type> _requiredAttributes;

    public DecoratedTypeTester(bool allowInherited, params Type[] requiredAttributes )
    {
        _allowInherited = allowInherited;

        _requiredAttributes = requiredAttributes.Where( t => typeof( Attribute ).IsAssignableFrom( t ) )
                                                .ToList();
    }

    public override bool MeetsRequirements( Type toCheck )
    {
        if( !base.MeetsRequirements( toCheck ) )
            return false;

        var typeAttributes = toCheck.GetCustomAttributes( _allowInherited );

        foreach( var attributeType in _requiredAttributes )
        {
            if( !typeAttributes.Any( x => attributeType.IsAssignableFrom( x.GetType() ) ) )
                return false;
        }

        return true;
    }
}
