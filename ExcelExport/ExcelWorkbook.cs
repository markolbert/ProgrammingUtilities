#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ExcelWorkbook.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using NPOI.XSSF.UserModel;

namespace J4JSoftware.Excel;

public class ExcelWorkbook : IEnumerable<ExcelSheet>
{
    private readonly ILogger? _logger;
    private readonly ILoggerFactory? _loggerFactory;
    private readonly List<ExcelSheet> _worksheets = new();
    private readonly XSSFWorkbook _xssfWorkbook;
    private int _activeSheetIndex = -1;
    private FileStream? _excelStream;

    private string? _filePath;

    public ExcelWorkbook( string? filePath = null,
        ILoggerFactory? loggerFactory = null )
    {
        _loggerFactory = loggerFactory;

        _xssfWorkbook = new XSSFWorkbook();

        _logger = _loggerFactory?.CreateLogger<ExcelWorkbook>();

        FilePath = filePath;
    }

    public ExcelWorkbook( ILoggerFactory? loggerFactory = null )
        : this( null, loggerFactory )
    {
    }

    public string? FilePath
    {
        get => _filePath;

        set
        {
            if( _excelStream != null )
            {
                Save();

                _excelStream.Flush();
                _excelStream.Close();

                _excelStream = null;
            }

            if( string.IsNullOrEmpty( value ) )
            {
                _filePath = null;
                return;
            }

            try
            {
                _excelStream = File.Exists( value )
                    ? File.Open( value, FileMode.OpenOrCreate, FileAccess.ReadWrite )
                    : File.Create( value );

                _filePath = value;
            }
            catch
            {
                _logger?.LogError( "Couldn't open or create file '{file}'", value );
            }
        }
    }

    public ReadOnlyCollection<ExcelSheet> Worksheets => _worksheets.AsReadOnly();

    public ExcelSheet? ActiveWorksheet
    {
        get
        {
            if( _worksheets.Count == 0 )
            {
                _logger?.LogError( "No worksheets are defined" );
                return null;
            }

            if( _activeSheetIndex < 0 || _activeSheetIndex >= _worksheets.Count - 1 )
            {
                _logger?.LogError( "No active worksheet defined" );
                return null;
            }

            return _worksheets[ _activeSheetIndex ];
        }
    }

    public ExcelSheet? this[ string name ]
    {
        get
        {
            var retVal =
                Worksheets.FirstOrDefault( w =>
                                               w.Sheet?.SheetName.Equals( name, StringComparison.OrdinalIgnoreCase )
                                            ?? false );

            if( retVal == null )
                _logger?.LogError( "Could not find worksheet '{name}'", name );

            return retVal;
        }
    }

    public IEnumerator<ExcelSheet> GetEnumerator()
    {
        foreach( var sheet in _worksheets ) yield return sheet;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool ActivateWorksheet( string name )
    {
        var idx = _worksheets.FindIndex( x =>
                                             x.Sheet?.SheetName.Equals( name, StringComparison.OrdinalIgnoreCase )
                                          ?? false );

        if( idx < 0 )
        {
            _logger?.LogError( "No worksheet named '{name}' exists in the workbook", name );
            return false;
        }

        _activeSheetIndex = idx;

        return true;
    }

    public bool AddWorksheet( string name, out ExcelSheet? result )
    {
        result = null;

        if( Worksheets.Any( w => w.Sheet?.SheetName.Equals( name, StringComparison.OrdinalIgnoreCase ) ?? false ) )
        {
            _logger?.LogError( "Duplicate worksheet name '{name}'", name );
            return false;
        }

        result = new ExcelSheet( _xssfWorkbook.CreateSheet( name ), _loggerFactory );
        _worksheets.Add( result );

        return ActivateWorksheet( name );
    }

    public bool Save()
    {
        if( _excelStream == null )
        {
            _logger?.LogError( "ExcelWorkbook is not linked to a file, set FilePath property" );
            return false;
        }

        _xssfWorkbook.Write( _excelStream );

        return true;
    }
}