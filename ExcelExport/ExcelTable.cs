#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ExcelTable.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Excel;

public class ExcelTable
{
    private readonly ILogger? _logger;

    internal ExcelTable( ExcelSheet sheet,
        int upperLeftRow,
        int upperLeftColumn,
        TableOrientation orientation,
        ILoggerFactory? loggerFactory = null )
    {
        ExcelSheet = sheet;

        _logger = loggerFactory?.CreateLogger<ExcelTable>();

        if( upperLeftRow < 0 )
            throw new ArgumentException( $"Upper left row cannot be < 0 ({upperLeftRow})" );

        if( upperLeftColumn < 0 )
            throw new ArgumentException( $"Upper left column cannot be < 0 ({upperLeftColumn})" );

        ExcelSheet = sheet;
        UpperLeftRow = upperLeftRow;
        UpperLeftColumn = upperLeftColumn;

        Orientation = orientation;
    }

    public bool IsValid => ExcelSheet != null;
    public ExcelSheet? ExcelSheet { get; }
    public int UpperLeftRow { get; }
    public int UpperLeftColumn { get; }
    public TableOrientation Orientation { get; }

    public int NumHeaders { get; private set; }
    public int NumEntries { get; private set; }

    public bool AutoSize()
    {
        if( !ValidateTable() )
            return false;

        var columns = Orientation == TableOrientation.ColumnHeaders ? NumHeaders : NumEntries;

        for( var column = 0; column < columns; column++ ) ExcelSheet!.Sheet!.AutoSizeColumn( column );

        return true;
    }

    public bool AddHeader( string header )
    {
        if( !ValidateTable() )
            return false;

        ExcelSheet!.MoveTo( UpperLeftRow + ( Orientation == TableOrientation.ColumnHeaders ? 0 : NumHeaders ),
                            UpperLeftColumn + ( Orientation == TableOrientation.RowHeaders ? 0 : NumHeaders ) );

        ExcelSheet.ActiveCell!.SetCellValue( header );

        NumHeaders++;

        return true;
    }

    public bool AddHeaders( params string[] headers )
    {
        if( !ValidateTable() )
            return false;

        foreach( var header in headers )
            if( !AddHeader( header ) )
                return false;

        return true;
    }

    public bool AddEntry<TEntity>( TEntity entity )
    {
        if( !ValidateTable() )
            return false;

        var values = typeof( TEntity ).GetProperties()
                                      .Select( p => p.GetValue( entity ) )
                                      .ToList();

        if( values.Count == 0 )
        {
            _logger?.LogError( "To properties found on instance of type '{entity}' to add to the table",
                            typeof( TEntity ) );
            return false;
        }

        AddEntry( values );

        return true;
    }

    public bool AddEntry( params object[] values )
    {
        return ValidateTable() && AddEntry( new List<object?>( values ) );
    }

    public bool AddEntry( List<object?> values )
    {
        if( !ValidateTable() )
            return false;

        ExcelSheet!.MoveTo( UpperLeftRow + ( Orientation == TableOrientation.RowHeaders ? 0 : NumEntries + 1 ),
                            UpperLeftColumn
                          + ( Orientation == TableOrientation.ColumnHeaders ? 0 : NumEntries + 1 ) );

        for( var idx = 0; idx < ( values.Count > NumHeaders ? values.Count : NumHeaders ); idx++ )
        {
            if( idx < values.Count && values[ idx ] != null )
                ExcelSheet.ActiveCell!.SetValue( values[ idx ]! );

            ExcelSheet.Move( Orientation == TableOrientation.RowHeaders ? 1 : 0,
                             Orientation == TableOrientation.ColumnHeaders ? 1 : 0 );
        }

        NumEntries++;

        ExcelSheet.Move( Orientation == TableOrientation.RowHeaders ? -values.Count : 0,
                         Orientation == TableOrientation.ColumnHeaders ? -values.Count : 0 );

        return true;
    }

    private bool ValidateTable()
    {
        if( !IsValid )
        {
            _logger?.LogError( "Table is invalid" );
            return false;
        }

        if( !ExcelSheet!.IsValid )
        {
            _logger?.LogError( "ExcelSheet is invalid" );
            return false;
        }

        return true;
    }
}