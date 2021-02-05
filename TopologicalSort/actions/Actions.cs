using System.Collections.Generic;
using J4JSoftware.Logging;

namespace J4JSoftware.Utilities
{
    public abstract class Actions<TSource> : Nodes<IAction<TSource>>, IAction<TSource>
    {
        protected Actions( 
            ActionsContext context,
            IJ4JLogger? logger = null
            )
        {
            Context = context;

            Logger = logger;
            Logger?.SetLoggedType( this.GetType() );
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

        protected virtual bool Initialize( TSource src ) => true;
        protected virtual bool Finalize( TSource src ) => true;

        // processors are equal if they are the same type, so duplicate instances of the 
        // same type are always equal (and shouldn't be present in the processing set)
        public bool Equals( IAction<TSource>? other )
        {
            if (other == null)
                return false;

            return other.GetType() == GetType();
        }

        bool IAction.Process( object src )
        {
            if( src is TSource castSrc )
                return Process( castSrc );

            Logger?.Error( "Expected a '{0}' but got a '{1}'", typeof(IAction<TSource>), src.GetType() );

            return false;
        }
    }
}