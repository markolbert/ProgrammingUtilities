using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.Excel
{
    public class ExcelSheet
    {
        private readonly Func<IJ4JLogger>? _loggerFactory;
        private readonly IJ4JLogger? _logger;
        private readonly List<IRow> _rows = new List<IRow>();
        private readonly List<ICell> _cells = new List<ICell>();

        internal ExcelSheet( 
            ISheet xssfSheet,
            Func<IJ4JLogger>? loggerFactory = null )
        {
            _loggerFactory = loggerFactory;

            _logger = _loggerFactory?.Invoke();
            _logger?.SetLoggedType( this.GetType() );

            Sheet = xssfSheet;
        }

        public bool IsValid => Sheet != null;
        public ISheet? Sheet { get; }
        public int ActiveRowNumber { get; private set; }
        public int ActiveColumnNumber { get; private set; }

        public ICell? this[ int row, int col ]
        {
            get
            {
                if( !IsValid )
                {
                    _logger?.Error("Worksheet is not initialized");
                    return null;
                }

                if ( row < 0 )
                {
                    _logger?.Error( "Invalid row# {0}", row );
                    return null;
                }

                if( col < 0 )
                {
                    _logger?.Error( "Invalid column #{0}", col );
                    return null;
                }

                var theRow = _rows.FirstOrDefault( r => r.RowNum == row );

                if( theRow == null )
                {
                    theRow = Sheet!.CreateRow( row );
                    _rows.Add( theRow );
                }

                var retVal = _cells.FirstOrDefault( c => c.RowIndex == row && c.ColumnIndex == col );

                if( retVal != null )
                    return retVal;

                retVal = theRow.CreateCell( col );
                _cells.Add( retVal );

                return retVal;
            }
        }

        public IRow? ActiveRow
        {
            get
            {
                if (!IsValid)
                {
                    _logger?.Error("Worksheet is not initialized");
                    return null;
                }

                var retVal = _rows.FirstOrDefault( r => r.RowNum == ActiveRowNumber );

                if( retVal != null ) 
                    return retVal;

                retVal = Sheet!.CreateRow( ActiveRowNumber );
                _rows.Add( retVal );

                return retVal;
            }
        }

        public ICell? ActiveCell
        {
            get
            {
                if (!IsValid)
                {
                    _logger?.Error("Worksheet is not initialized");
                    return null;
                }

                var row = ActiveRow;

                var retVal = _cells.FirstOrDefault( c => c.RowIndex == row!.RowNum
                                                         && c.ColumnIndex == ActiveColumnNumber );

                if( retVal != null ) 
                    return retVal;
                
                retVal = row!.CreateCell( ActiveColumnNumber );
                _cells.Add( retVal );

                return retVal;
            }
        }

        public ExcelSheet MoveTo( int row, int col )
        {
            if( row < 0 )
            {
                _logger?.Error( "Row # cannot be < 0 ({0})", row );
                return this;
            }

            if( col < 0 )
            {
                _logger?.Error( "{Column # cannot be < 0 ({0})", col );
                return this;
            }

            ActiveRowNumber = row;
            ActiveColumnNumber = col;

            return this;
        }

        public ExcelSheet Move( int rows, int cols )
        {
            if( rows + ActiveRowNumber < 0 )
            {
                _logger?.Error( "Cannot move before row 0 ({0})", rows );
                return this;
            }

            if( cols + ActiveColumnNumber < 0 )
            {
                _logger?.Error( "Cannot move before column 0 ({0})", cols );
                return this;
            }

            ActiveRowNumber += rows;
            ActiveColumnNumber += cols;

            return this;
        }

        public ExcelSheet AddNameValueRow( string name, object value )
        {
            if (!IsValid)
            {
                _logger?.Error("Worksheet is not initialized");
                return this;
            }

            ActiveCell!.SetCellValue(name);
            ActiveColumnNumber++;

            ActiveCell.SetValue( value );

            ActiveColumnNumber--;
            ActiveRowNumber++;

            return this;
        }

        public bool AddTable( int upperLeftRow,
            int upperLeftColumn,
            TableOrientation orientation,
            out ExcelTable? result )
        {
            result = null;

            if( upperLeftRow < 0 )
            {
                _logger?.Error( "Upper left row cannot be < 0 ({0})", upperLeftRow );
                return false;
            }

            if( upperLeftColumn < 0 )
            {
                _logger?.Error( "Upper left column cannot be < 0 ({0})", upperLeftColumn );
                return false;
            }

            result = new ExcelTable( this, upperLeftRow, upperLeftColumn, orientation, _loggerFactory?.Invoke() );

            return true;
        }
    }
}