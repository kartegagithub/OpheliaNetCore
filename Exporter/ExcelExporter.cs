using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Ophelia.Data.Exporter.Controls;
using System;
using System.Collections.Generic;

namespace Ophelia.Data.Exporter
{
    public class ExcelExporter : IExporter
    {
        public byte[] Export(Grid grid)
        {
            var grids = new List<Grid>();
            grids.Add(grid);
            return this.Export(grids);
        }

        public byte[] Export(List<Grid> grids)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
            {
                WriteExcelFile(grids, document);
            }
            grids.Clear();
            grids = null;
            stream.Flush();
            stream.Position = 0;

            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            stream.Close();
            stream.Dispose();
            return data;
        }
        private static void WriteExcelFile(List<Controls.Grid> grids, SpreadsheetDocument spreadsheet)
        {
            //  Create the Excel file contents.  This function is used when creating an Excel file either writing 
            //  to a file, or writing to a MemoryStream.
            spreadsheet.AddWorkbookPart();
            spreadsheet.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

            //  My thanks to James Miera for the following line of code (which prevents crashes in Excel 2010)
            spreadsheet.WorkbookPart.Workbook.Append(new BookViews(new WorkbookView()));

            //  If we don't add a "WorkbookStylesPart", OLEDB will refuse to connect to this .xlsx file !
            WorkbookStylesPart workbookStylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>("rIdStyles");
            Stylesheet stylesheet = new Stylesheet();
            workbookStylesPart.Stylesheet = stylesheet;

            //  Loop through each of the DataTables in our DataSet, and create a new Excel Worksheet for each.
            uint worksheetNumber = 1;
            foreach (var grid in grids)
            {
                //  For each worksheet you want to create
                string workSheetID = "rId" + worksheetNumber.ToString();
                string worksheetName = grid.Text;

                WorksheetPart newWorksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
                newWorksheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet();

                // create sheet data
                newWorksheetPart.Worksheet.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.SheetData());

                // save worksheet
                WriteDataTableToExcelWorksheet(grid, newWorksheetPart);
                newWorksheetPart.Worksheet.Save();

                // create the worksheet to workbook relation
                if (worksheetNumber == 1)
                    spreadsheet.WorkbookPart.Workbook.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Sheets());

                spreadsheet.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>().AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Sheet()
                {
                    Id = spreadsheet.WorkbookPart.GetIdOfPart(newWorksheetPart),
                    SheetId = worksheetNumber,
                    Name = grid.Text
                });

                worksheetNumber++;
            }

            spreadsheet.WorkbookPart.Workbook.Save();
        }


        private static void WriteDataTableToExcelWorksheet(Controls.Grid grid, WorksheetPart worksheetPart)
        {
            var worksheet = worksheetPart.Worksheet;
            var sheetData = worksheet.GetFirstChild<SheetData>();

            int numberOfColumns = grid.Columns.Count;

            string[] excelColumnNames = new string[numberOfColumns];
            for (int n = 0; n < numberOfColumns; n++)
                excelColumnNames[n] = GetExcelColumnName(n);

            uint rowIndex = 1;

            var headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row { RowIndex = rowIndex };
            sheetData.Append(headerRow);

            int colInx = 0;
            foreach (var col in grid.Columns)
            {
                AppendCell(excelColumnNames[colInx] + "1", col.Text, headerRow);
                colInx++;
            }

            foreach (var row in grid.Rows)
            {
                ++rowIndex;
                var newExcelRow = new DocumentFormat.OpenXml.Spreadsheet.Row { RowIndex = rowIndex };
                sheetData.Append(newExcelRow);
                colInx = 0;
                foreach (var cell in row.Cells)
                {
                    var cellValue = Convert.ToString(cell.Value);
                    if (cell.Column.IsNumeric)
                        AppendCell(excelColumnNames[colInx] + rowIndex.ToString(), cellValue, newExcelRow, CellValues.Number);
                    else
                        AppendCell(excelColumnNames[colInx] + rowIndex.ToString(), cellValue, newExcelRow);
                    cellValue = null;

                    colInx++;
                }
            }
        }

        private static void AppendCell(string cellReference, string cellStringValue, DocumentFormat.OpenXml.Spreadsheet.Row excelRow, CellValues type = CellValues.String)
        {
            //  Add a new Excel Cell to our Row 
            var cell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = cellReference, DataType = type };
            CellValue cellValue = new CellValue();
            cellValue.Text = cellStringValue;
            cell.Append(cellValue);
            excelRow.Append(cell);
        }

        private static string GetExcelColumnName(int columnIndex)
        {
            if (columnIndex < 26)
                return ((char)('A' + columnIndex)).ToString();

            char firstChar = (char)('A' + (columnIndex / 26) - 1);
            char secondChar = (char)('A' + (columnIndex % 26));

            return string.Format("{0}{1}", firstChar, secondChar);
        }

        private static Type GetNullableType(Type t)
        {
            Type returnType = t;
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                returnType = Nullable.GetUnderlyingType(t);
            }
            return returnType;
        }
        private static bool IsNullableType(Type type)
        {
            return (type == typeof(string) || type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))));
        }
    }
}
