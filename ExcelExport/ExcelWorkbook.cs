using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using J4JSoftware.Logging;
using NPOI.XSSF.UserModel;

namespace J4JSoftware.Excel
{
    public class ExcelWorkbook : IEnumerable<ExcelSheet>
    {
        private readonly Func<ExcelSheet> _sheetFactory;
        private readonly List<ExcelSheet> _worksheets = new List<ExcelSheet>();
        private readonly IJ4JLogger _logger;

        private FileStream? _excelStream;
        private int _activeSheetIndex = -1;

        public ExcelWorkbook(  
            Func<ExcelSheet> sheetFactory,
            IJ4JLogger logger 
            )
        {
            _sheetFactory = sheetFactory;

            _logger = logger;
            _logger.SetLoggedType( this.GetType() );
        }

        public bool IsValid => _excelStream != null;
        public ReadOnlyCollection<ExcelSheet> Worksheets => _worksheets.AsReadOnly();

        internal XSSFWorkbook? WorkbookInternal { get; private set; }

        public ExcelSheet? ActiveWorksheet
        {
            get
            {
                if( _worksheets.Count == 0 )
                {
                    _logger.Error("No worksheets are defined");
                    return null;
                }

                if( _activeSheetIndex < 0 || _activeSheetIndex >= ( _worksheets.Count - 1 ) )
                {
                    _logger.Error("No active worksheet defined");
                    return null;
                }

                return _worksheets[ _activeSheetIndex ];
            }
        }

        public bool ActivateWorksheet( string name )
        {
            var idx = _worksheets.FindIndex( x =>
                x.Sheet?.SheetName.Equals( name, StringComparison.OrdinalIgnoreCase ) ?? false );

            if( idx < 0 )
            {
                _logger.Error<string>( "No worksheet named '{0}' exists in the workbook", name );
                return false;
            }

            _activeSheetIndex = idx;

            return true;
        }

        public ExcelSheet? this[ string name ]
        {
            get
            {
                var retVal =
                    Worksheets.FirstOrDefault( w =>
                        w.Sheet?.SheetName.Equals( name, StringComparison.OrdinalIgnoreCase ) ?? false );

                if( retVal == null )
                    _logger.Error<string>("Could not find worksheet '{0}'", name);

                return retVal;
            }
        }

        public bool Open( string filePath )
        {
            try
            {
                _excelStream = File.Create( filePath );
            }
            catch
            {
                _logger.Error<string>( "Invalid valid name '{0}'", filePath );
            }

            if( IsValid )
            {
                WorkbookInternal = new XSSFWorkbook();

                _logger.Information( $"Opened Excel file '{_excelStream!.Name}', created workbook" );
            }
            else WorkbookInternal = null;

            return IsValid;
        }

        public bool AddWorksheet( string name, out ExcelSheet? result )
        {
            result = null;

            if( WorkbookInternal == null )
            {
                _logger.Error<string>( "Workbook '{0}' is not defined", name );
                return false;
            }

            if( Worksheets.Any( w => w.Sheet?.SheetName.Equals( name, StringComparison.OrdinalIgnoreCase ) ?? false ) )
            {
                _logger.Error<string>( "Duplicate worksheet name '{0}'", name );
                return false;
            }

            var worksheet = _sheetFactory();

            if( worksheet.Initialize( this, name ) )
            {
                _worksheets.Add( worksheet );
                result = worksheet;

                return ActivateWorksheet(name);
            }

            return false;
        }

        public bool Close()
        {
            if( !IsValid )
            {
                _logger.Error( $"{nameof(ExcelWorkbook)} is invalid, can't close workbook" );
                return false;
            }

            WorkbookInternal!.Write( _excelStream );
            WorkbookInternal = null;

            return true;
        }

        public IEnumerator<ExcelSheet> GetEnumerator()
        {
            if( !IsValid )
                yield break;

            foreach( var sheet in Worksheets )
            {
                yield return sheet;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
