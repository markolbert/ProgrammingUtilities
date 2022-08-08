#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'TopologicalSort' is free software: you can redistribute it
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

using J4JSoftware.Logging;

namespace J4JSoftware.Utilities;

public abstract class Actions<TSource> : Nodes<IAction<TSource>>, IAction<TSource>
{
    protected Actions( ActionsContext context,
        IJ4JLogger? logger = null )
    {
        Context = context;

        Logger = logger;
        Logger?.SetLoggedType( GetType() );
    }

    protected IJ4JLogger? Logger { get; }
    protected ActionsContext Context { get; }

    public virtual bool Process( TSource src )
    {
        if( !Initialize( src ) )
            return false;

        var allOkay = true;

        if( !Sort( out var actions, out var _ ) )
        {
            Logger?.Error( "Couldn't topologically sort actions" );
            return false;
        }

        actions!.Reverse();

        foreach( var action in actions! )
        {
            allOkay &= action.Process( src );

            if( !allOkay && Context.StopOnFirstError )
                break;
        }

        return allOkay && Finalize( src );
    }

    // processors are equal if they are the same type, so duplicate instances of the 
    // same type are always equal (and shouldn't be present in the processing set)
    public bool Equals( IAction<TSource>? other )
    {
        if( other == null )
            return false;

        return other.GetType() == GetType();
    }

    bool IAction.Process( object src )
    {
        if( src is TSource castSrc )
            return Process( castSrc );

        Logger?.Error( "Expected a '{0}' but got a '{1}'", typeof( IAction<TSource> ), src.GetType() );

        return false;
    }

    protected virtual bool Initialize( TSource src )
    {
        return true;
    }

    protected virtual bool Finalize( TSource src )
    {
        return true;
    }
}