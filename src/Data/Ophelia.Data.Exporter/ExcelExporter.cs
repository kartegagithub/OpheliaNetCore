using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Ophelia.Data.Exporter.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ophelia.Data.Exporter
{
    public class ExcelExporter : IExporter
    {
        public bool AutoSizeColumns { get; set; }

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
        private void WriteExcelFile(List<Controls.Grid> grids, SpreadsheetDocument spreadsheet)
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


        private void WriteDataTableToExcelWorksheet(Controls.Grid grid, WorksheetPart worksheetPart)
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
                AppendCell(excelColumnNames[colInx] + "1", col.Text, headerRow, CellValues.String);
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
                    AppendCell(excelColumnNames[colInx] + rowIndex.ToString(), cellValue, newExcelRow, CellValues.String);
                    cellValue = null;

                    colInx++;
                }
            }

            if (this.AutoSizeColumns)
            {
                var columns = AutoSize(sheetData);
                worksheet.InsertAt(columns, 0);
            }
        }

        private void AppendCell(string cellReference, string cellStringValue, DocumentFormat.OpenXml.Spreadsheet.Row excelRow, CellValues type)
        {
            //  Add a new Excel Cell to our Row 
            var cell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = cellReference, DataType = type };
            CellValue cellValue = new CellValue();
            cellValue.Text = cellStringValue;
            if (!string.IsNullOrEmpty(cellValue.Text) && (cellValue.Text.StartsWith('=') || cellValue.Text.StartsWith('@')))
                cellValue.Text = $" ' {cellValue.Text}";

            if (cellValue.Text.IsNumeric())
            {
                cellValue.Text = cellValue.Text.Trim().TrimStart('+');
                cell.DataType = CellValues.Number;
            }

            cell.Append(cellValue);
            excelRow.Append(cell);
        }

        private string GetExcelColumnName(int columnIndex)
        {
            if (columnIndex < 26)
                return ((char)('A' + columnIndex)).ToString();

            char firstChar = (char)('A' + (columnIndex / 26) - 1);
            char secondChar = (char)('A' + (columnIndex % 26));

            return string.Format("{0}{1}", firstChar, secondChar);
        }

        private Type GetNullableType(Type t)
        {
            Type returnType = t;
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                returnType = Nullable.GetUnderlyingType(t);
            }
            return returnType;
        }
        private bool IsNullableType(Type type)
        {
            return (type == typeof(string) || type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))));
        }

        private Columns AutoSize(SheetData sheetData)
        {
            var maxColWidth = GetMaxCharacterWidth(sheetData);

            Columns columns = new Columns();
            //this is the width of my font - yours may be different
            double maxWidth = 7;
            foreach (var item in maxColWidth)
            {
                //width = Truncate([{Number of Characters} * {Maximum Digit Width} + {5 pixel padding}]/{Maximum Digit Width}*256)/256
                double width = Math.Truncate((item.Value * maxWidth + 5) / maxWidth * 256) / 256;

                //pixels=Truncate(((256 * {width} + Truncate(128/{Maximum Digit Width}))/256)*{Maximum Digit Width})
                double pixels = Math.Truncate(((256 * width + Math.Truncate(128 / maxWidth)) / 256) * maxWidth);

                //character width=Truncate(({pixels}-5)/{Maximum Digit Width} * 100+0.5)/100
                double charWidth = Math.Truncate((pixels - 5) / maxWidth * 100 + 0.5) / 100;

                var col = new DocumentFormat.OpenXml.Spreadsheet.Column() { BestFit = true, Min = (UInt32)(item.Key + 1), Max = (UInt32)(item.Key + 1), CustomWidth = true, Width = (DoubleValue)width };
                columns.Append(col);
            }

            return columns;
        }


        private Dictionary<int, int> GetMaxCharacterWidth(SheetData sheetData)
        {
            //iterate over all cells getting a max char value for each column
            Dictionary<int, int> maxColWidth = new Dictionary<int, int>();
            var rows = sheetData.Elements<DocumentFormat.OpenXml.Spreadsheet.Row>();
            UInt32[] numberStyles = new UInt32[] { 5, 6, 7, 8 }; //styles that will add extra chars
            UInt32[] boldStyles = new UInt32[] { 1, 2, 3, 4, 6, 7, 8 }; //styles that will bold
            foreach (var r in rows)
            {
                var cells = r.Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>().ToArray();

                //using cell index as my column
                for (int i = 0; i < cells.Length; i++)
                {
                    var cell = cells[i];
                    var cellValue = cell.CellValue == null ? string.Empty : cell.CellValue.InnerText;
                    var cellTextLength = cellValue.Length;

                    if (cell.StyleIndex != null && numberStyles.Contains(cell.StyleIndex))
                    {
                        int thousandCount = (int)Math.Truncate((double)cellTextLength / 4);

                        //add 3 for '.00' 
                        cellTextLength += (3 + thousandCount);
                    }

                    if (cell.StyleIndex != null && boldStyles.Contains(cell.StyleIndex))
                    {
                        //add an extra char for bold - not 100% acurate but good enough for what i need.
                        cellTextLength += 1;
                    }

                    if (maxColWidth.ContainsKey(i))
                    {
                        var current = maxColWidth[i];
                        if (cellTextLength > current)
                        {
                            maxColWidth[i] = cellTextLength;
                        }
                    }
                    else
                    {
                        maxColWidth.Add(i, cellTextLength);
                    }
                }
            }

            return maxColWidth;
        }
    }
}
