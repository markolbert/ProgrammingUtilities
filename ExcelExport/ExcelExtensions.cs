using System;
using NPOI.SS.UserModel;

namespace J4JSoftware.Excel
{
    public static class ExcelExtensions
    {
        public static ICell SetValue( this ICell cell, object value )
        {
            switch( value )
            {
                case bool xVal:
                    cell.SetCellValue( xVal );
                    break;

                case string xVal:
                    cell.SetCellValue( xVal );
                    break;

                case int xVal:
                    cell.SetCellValue( xVal );
                    break;

                case double xVal:
                    cell.SetCellValue( xVal );
                    break;

                case IRichTextString xVal:
                    cell.SetCellValue( xVal );
                    break;

                case DateTime xVal:
                    cell.SetCellValue( xVal );
                    break;
            }

            return cell;
        }
    }
}