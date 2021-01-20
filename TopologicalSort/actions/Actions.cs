using System.Collections.Generic;
using J4JSoftware.Logging;

namespace J4JSoftware.Utilities
{
    public class Actions<TSymbol> : Nodes<IAction<TSymbol>>, IActionProcessor<TSymbol>
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

        // symbols must be able to reset so it can be iterated multiple times
        public virtual bool Process( IEnumerable<TSymbol> symbols )
        {
            if( !Initialize( symbols ) )
                return false;

            var allOkay = true;

            if( !Sort( out var procesorNodes, out var _ ) )
            {
                Logger?.Error( "Couldn't topologically sort processors" );
                return false;
            }

            procesorNodes!.Reverse();

            foreach( var node in procesorNodes! )
            {
                allOkay &= node.Process( symbols );

                if( !allOkay && Context.StopOnFirstError )
                    break;
            }

            return allOkay && Finalize( symbols );
        }

        protected virtual bool Initialize( IEnumerable<TSymbol> symbols ) => true;
        protected virtual bool Finalize( IEnumerable<TSymbol> symbols ) => true;
    }
}