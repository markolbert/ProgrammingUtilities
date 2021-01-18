using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Excel
{
    public class ExcelTable
    {
        private readonly IJ4JLogger? _logger;

        public ExcelTable( IJ4JLogger? logger = null )
        {
            _logger = logger ?? throw new NullReferenceException( nameof(logger) );
            _logger?.SetLoggedType( this.GetType() );
        }

        public bool IsValid => ExcelSheet != null;
        public ExcelSheet? ExcelSheet { get; private set; }
        public int UpperLeftRow { get; private set; }
        public int UpperLeftColumn { get; private set; }
        public TableOrientation Orientation { get; private set; }

        public int NumHeaders { get; private set; }
        public int NumEntries { get; private set; }

        public bool Initialize( ExcelSheet sheet, int upperLeftRow, int upperLeftColumn, TableOrientation orientation )
        {
            if( upperLeftRow < 0 )
            {
                _logger?.Error("Upper left row cannot be < 0 ({0})", upperLeftRow);
                return false;
            }

            if( upperLeftColumn < 0 )
            {
                _logger?.Error("Upper left column cannot be < 0 ({0})", upperLeftColumn);
                return false;
            }

            ExcelSheet = sheet;
            UpperLeftRow = upperLeftRow;
            UpperLeftColumn = upperLeftColumn;

            Orientation = orientation;

            return true;
        }

        public bool AutoSize()
        {
            if( !ValidateTable() )
                return false;

            var columns = Orientation == TableOrientation.ColumnHeaders ? NumHeaders : NumEntries;

            for( var column = 0; column < columns; column++ )
            {
                ExcelSheet!.Sheet!.AutoSizeColumn( column );
            }

            return true;
        }

        public bool AddHeader( string header )
        {
            if (!ValidateTable())
                return false;

            ExcelSheet!.MoveTo(
                UpperLeftRow + ( Orientation == TableOrientation.ColumnHeaders ? 0 : NumHeaders ),
                UpperLeftColumn + ( Orientation == TableOrientation.RowHeaders ? 0 : NumHeaders )
            );

            ExcelSheet.ActiveCell!.SetCellValue( header );

            NumHeaders++;

            return true;
        }

        public bool AddHeaders( params string[] headers )
        {
            if (!ValidateTable())
                return false;

            foreach ( var header in headers )
            {
                if( !AddHeader( header ) )
                    return false;
            }

            return true;
        }

        public bool AddEntry<TEntity>( TEntity entity )
        {
            if (!ValidateTable())
                return false;

            var values = typeof(TEntity).GetProperties()
                .Select( p => p.GetValue( entity ) )
                .ToList();

            if( values.Count == 0 )
            {
                _logger?.Error( "To properties found on instance of type '{0}' to add to the table", typeof(TEntity) );
                return false;
            }

            AddEntry( values );

            return true;
        }

        public bool AddEntry( params object[] values ) => ValidateTable() && AddEntry( new List<object?>( values ) );

        public bool AddEntry( List<object?> values )
        {
            if (!ValidateTable())
                return false;

            ExcelSheet!.MoveTo(
                UpperLeftRow + ( Orientation == TableOrientation.RowHeaders ? 0 : NumEntries + 1),
                UpperLeftColumn + ( Orientation == TableOrientation.ColumnHeaders ? 0 : NumEntries + 1 )
            );

            for ( var idx = 0; idx < ( values.Count > NumHeaders ? values.Count : NumHeaders ); idx++ )
            {
                if( idx < values.Count && values[idx] != null )
                    ExcelSheet.ActiveCell!.SetValue( values[idx]! );

                ExcelSheet.Move(
                    Orientation == TableOrientation.RowHeaders ? 1 : 0,
                    Orientation == TableOrientation.ColumnHeaders ? 1 : 0
                );
            }

            NumEntries++;

            ExcelSheet.Move(
                Orientation == TableOrientation.RowHeaders ? -values.Count : 0,
                Orientation == TableOrientation.ColumnHeaders ? -values.Count : 0
            );

            return true;
        }

        private bool ValidateTable()
        {
            if (!IsValid)
            {
                _logger?.Error("Table is invalid");
                return false;
            }

            if (!ExcelSheet!.IsValid)
            {
                _logger?.Error("ExcelSheet is invalid");
                return false;
            }

            return true;
        }
    }
}