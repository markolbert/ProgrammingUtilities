using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Alba.CsConsoleFormat;
using Alba.CsConsoleFormat.Fluent;
using J4JSoftware.Logging;

namespace J4JSoftware.ConsoleUtilities
{
    public abstract class PropertyUpdater<TProp> : IPropertyUpdater<TProp>
    {
        protected PropertyUpdater( IJ4JLogger? logger )
        {
            Logger = logger;
            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }

        public string Indent { get; set; } = new string( ' ', 4 );

        public abstract UpdaterResult Validate( TProp? origValue, out TProp? newValue );

        public Type ValidatorType => typeof(TProp);

        UpdaterResult IPropertyUpdater.Update( object? origValue, out object? newValue )
        {
            newValue = origValue;

            if( origValue is TProp castValue )
            {
                var result = Validate( castValue, out var innerNew );

                if( result == UpdaterResult.Changed)
                    newValue = innerNew;

                return result;
            }

            Logger?.Error( "Expected a {0} but got a {1}", typeof(TProp), origValue?.GetType() );

            return UpdaterResult.InvalidValidator;
        }

        protected T GetEnum<T>(T curValue, T defaultValue, List<T>? values = null, IJ4JLogger? logger = null )
            where T : Enum
        {
            Colors.WriteLine( "Enter ", 
                typeof(T).Name.Yellow(), 
                " (current value is ".White(),
                curValue.ToString().Green(), 
                ") :\n" );

            values ??= Enum.GetValues(typeof(T)).Cast<T>().ToList();

            for (var idx = 0; idx < values.Count; idx++)
            {
                Colors.WriteLine(Indent,
                    (idx + 1).ToString().Green(),
                    " - ",
                    values[idx].ToString());
            }

            Console.Write("\n\nChoice: ");

            var text = Console.ReadLine();

            if (!string.IsNullOrEmpty(text)
                && int.TryParse(text, NumberStyles.Integer, null, out var choice)
                && choice >= 1
                && choice <= values.Count)
                return values[choice - 1];

            return defaultValue;
        }

        protected T? GetSingleValue<T>( T curValue, string prompt, T? defaultValue = default )
        {
            defaultValue ??= curValue;

            Colors.WriteLine( "Enter ", 
                prompt.Green(), 
                " (current value is '".White(),
                ( curValue == null ? "**undefined**" : curValue.ToString()! ).Green(), "'): " );

            Console.Write("> ");

            var userInput = Console.ReadLine();

            if( string.IsNullOrEmpty( userInput ) )
                userInput = defaultValue?.ToString();

            try
            {
                var retVal = Convert.ChangeType( userInput, typeof(T) );

                return retVal == null ? defaultValue : (T) retVal;
            }
            catch
            {
                return defaultValue;
            }
        }

        protected List<T> GetMultipleValues<T>( List<T> curValue, string prompt )
        {
            Colors.WriteLine( "Enter one or more ", 
                prompt.Green(), 
                " (current value is '".White(),
                string.Join(", ", curValue  ).Green(), "'): " );


            var values = new List<string>();

            while( true )
            {
                Console.Write("(hit return to end input) > ");
                
                var userInput = Console.ReadLine();

                if( string.IsNullOrEmpty( userInput ) )
                    break;

                values.AddRange( SplitInput( userInput ) );
            }

            if( values.Count == 0 )
                return new List<T>();

            try
            {
                return values.Select( x => Convert.ChangeType( x, typeof(T) ) )
                    .Cast<T>()
                    .ToList();
            }
            catch
            {
                return new List<T>();
            }
        }

        private IEnumerable<string> SplitInput( string input )
        {
            bool foundQuote = false;
            var sb = new StringBuilder();

            foreach( var curChar in input )
            {
                switch( curChar )
                {
                    case ' ':
                        if( !foundQuote )
                        {
                            yield return sb.ToString();
                            sb.Clear();
                        }
                        else sb.Append( curChar );

                        break;

                    case '"':
                        foundQuote = !foundQuote;
                        break;

                    default:
                        sb.Append( curChar );
                        break;

                }
            }

            if( sb.Length > 0 )
                yield return sb.ToString();
        }

        protected string GetText(string curValue, string prompt, string? defaultValue = null )
        {
            defaultValue ??= curValue;

            Colors.WriteLine( "Enter ", prompt.Green(), " (current value is '".White(), curValue.Green(), "'): ");
            Console.Write("> ");

            var retVal = Console.ReadLine();

            return string.IsNullOrEmpty(retVal) ? defaultValue : retVal;
        }

        protected string GetText(string curValue, string? defaultValue, params Span[] prompts)
        {
            defaultValue ??= curValue;

            var promptList = prompts.ToList();
            promptList.Insert( 0, "Enter ".White() );
            promptList.Add(" (current value is '".White()  );
            promptList.Add(curValue.Green()  );
            promptList.Add( "') :".White() );

            Colors.WriteLine( promptList );
            Console.Write("> ");

            var retVal = Console.ReadLine();

            return string.IsNullOrEmpty(retVal) ? defaultValue : retVal;
        }
    }
}