using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Utilities
{
    public abstract class TopoAction<TItem> : IAction<TItem>
    {
        protected TopoAction(
            IJ4JLogger logger
        )
        {
            Logger = logger;
            Logger.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger Logger { get; }

        public bool Process( IEnumerable<TItem> inputData )
        {
            if( !Initialize( inputData ) )
                return false;

            if( !ProcessLoop( inputData ) )
                return false;

            return Finalize( inputData );
        }

        protected virtual bool Initialize(IEnumerable<TItem> inputData) => true;

        protected virtual bool Finalize(IEnumerable<TItem> inputData ) => true;

        protected abstract bool ProcessLoop( IEnumerable<TItem> inputData );

        // processors are equal if they are the same type, so duplicate instances of the 
        // same type are always equal (and shouldn't be present in the processing set)
        public bool Equals( IAction<TItem>? other )
        {
            if (other == null)
                return false;

            return other.GetType() == GetType();
        }
    }
}
