using System.Collections.Generic;
using J4JSoftware.Logging;

namespace J4JSoftware.Utilities
{
    public abstract class TopoAction<TSource> : IAction<TSource>
    {
        protected TopoAction(
            IJ4JLogger logger
        )
        {
            Logger = logger;
            Logger.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger Logger { get; }

        public bool Process( TSource src )
        {
            if( !Initialize( src ) )
                return false;

            if( !ProcessLoop( src ) )
                return false;

            return Finalize( src );
        }

        protected virtual bool Initialize(TSource src) => true;

        protected virtual bool Finalize(TSource src ) => true;

        protected abstract bool ProcessLoop( TSource src );

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
