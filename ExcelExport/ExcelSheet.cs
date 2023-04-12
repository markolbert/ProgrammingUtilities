#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ExcelSheet.cs
//
// This file is part of JumpForJoy Software's ExcelExport.
// 
// ExcelExport is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// ExcelExport is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with ExcelExport. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.Excel;

public class ExcelSheet
{
    private readonly List<ICell> _cells = new();
    private readonly ILogger? _logger;
    private readonly ILoggerFactory? _loggerFactory;
    private readonly List<IRow> _rows = new();

    internal ExcelSheet( ISheet xssfSheet,
        ILoggerFactory? loggerFactory = null )
    {
        _loggerFactory = loggerFactory;

        _logger = _loggerFactory?.CreateLogger<ExcelSheet>();

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
                _logger?.LogError( "Worksheet is not initialized" );
                return null;
            }

            if( row < 0 )
            {
                _logger?.LogError( "Invalid row# {row}", row );
                return null;
            }

            if( col < 0 )
            {
                _logger?.LogError( "Invalid column #{col}", col );
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
            if( !IsValid )
            {
                _logger?.LogError( "Worksheet is not initialized" );
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
            if( !IsValid )
            {
                _logger?.LogError( "Worksheet is not initialized" );
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
            _logger?.LogError( "Row # cannot be < 0 ({row})", row );
            return this;
        }

        if( col < 0 )
        {
            _logger?.LogError( "Column # cannot be < 0 ({col})", col );
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
            _logger?.LogError( "Cannot move before row 0 ({rows})", rows );
            return this;
        }

        if( cols + ActiveColumnNumber < 0 )
        {
            _logger?.LogError( "Cannot move before column 0 ({cols})", cols );
            return this;
        }

        ActiveRowNumber += rows;
        ActiveColumnNumber += cols;

        return this;
    }

    public ExcelSheet AddNameValueRow( string name, object value )
    {
        if( !IsValid )
        {
            _logger?.LogError( "Worksheet is not initialized" );
            return this;
        }

        ActiveCell!.SetCellValue( name );
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
            _logger?.LogError( "Upper left row cannot be < 0 ({upperLeftRow})", upperLeftRow );
            return false;
        }

        if( upperLeftColumn < 0 )
        {
            _logger?.LogError( "Upper left column cannot be < 0 ({upperLeftColumn})", upperLeftColumn );
            return false;
        }

        result = new ExcelTable( this, upperLeftRow, upperLeftColumn, orientation, _loggerFactory );

        return true;
    }
}