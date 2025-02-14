using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Ophelia.Data.Exporter.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Ophelia.Data.Exporter
{
    public class ExcelExporter : IExporter
    {
        public ExcelExporter()
        {
            this.FillCellFormats();
        }
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
            // spreadsheet.WorkbookPart.Workbook.Append(new BookViews(new WorkbookView()));

            //  If we don't add a "WorkbookStylesPart", OLEDB will refuse to connect to this .xlsx file !
            WorkbookStylesPart workbookStylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>("rIdStyles");
            workbookStylesPart.Stylesheet = this.GenerateStyleSheet();
            workbookStylesPart.Stylesheet.Save();

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
                    Name = grid.Text ?? $"Sheet{worksheetNumber}"
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
                AppendCell(excelColumnNames[colInx] + "1", col.Text, headerRow, col.Type.GetValueOrDefault(CellValues.String), null, -1);
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
                    AppendCell(excelColumnNames[colInx] + rowIndex.ToString(), cell.Value, newExcelRow, cell.Column.Type.GetValueOrDefault(CellValues.String), cell.Column.ValueType, cell.Column.StyleID);
                    colInx++;
                }
            }

            if (this.AutoSizeColumns)
            {
                var columns = AutoSize(sheetData);
                worksheet.InsertAt(columns, 0);
            }
        }

        private void AppendCell(string cellReference, object cellData, DocumentFormat.OpenXml.Spreadsheet.Row excelRow, CellValues type, Type cellDataType, int styleID)
        {
            //  Add a new Excel Cell to our Row 
            var cell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = cellReference, DataType = type };
            CellValue cellValue = new CellValue();
            if (cellDataType != null)
            {
                type = CellValues.String;
                if (cellDataType.IsNumeric())
                {
                    type = CellValues.Number;
                    if (styleID == -1)
                    {
                        styleID = 1;
                        if (cellDataType.IsDecimal() || cellDataType.IsDouble() || cellDataType.IsSingle()) styleID = 2;
                    }
                }
                if (cellDataType.IsDate())
                {
                    type = CellValues.Date;
                    if (cellData != null)
                        cellValue.Text = ((DateTime)cellData).ToString("s");
                    if (styleID == -1)
                    {
                        styleID = 22;
                        if (!string.IsNullOrEmpty(cellValue.Text) && cellValue.Text.IndexOf(" 00:00") > -1)
                        {
                            styleID = 14;
                        }
                    }
                }

                cell.DataType = type;
            }
            else
            {
                if (cellData != null && cellData.IsDate())
                {
                    cell.DataType = CellValues.Date;
                    if (cellData != null)
                        cellValue.Text = ((DateTime)cellData).ToString("s");
                    if (styleID == -1)
                    {
                        styleID = 22;
                        if (!string.IsNullOrEmpty(cellValue.Text) && cellValue.Text.IndexOf(" 00:00") > -1)
                        {
                            styleID = 14;
                        }
                    }
                }
                if (cellData != null && cellData.ToString().IsNumeric())
                {
                    cellValue.Text = cellData.ToString();
                    cell.DataType = CellValues.Number;
                    if (styleID == -1)
                    {
                        styleID = 1;
                        if (cellValue.Text.IndexOf('.') > -1 || cellValue.Text.IndexOf(',') > -1)
                        {
                            cellValue.Text = Convert.ToDecimal(cellData.ToString()).ToString(CultureInfo.InvariantCulture);
                            styleID = 2;
                        }
                    }
                }
            }
            if (styleID == -1) styleID = 0;

            cell.StyleIndex = UInt32Value.FromUInt32(this.GetStyleIndex(styleID));

            if (string.IsNullOrEmpty(cellValue.Text))
            {
                if (cellDataType.IsDecimal() && cellData != null)
                {
                    cellValue.Text = Convert.ToDecimal(cellData.ToString()).ToString(CultureInfo.InvariantCulture);
                }
                else if (cellDataType.IsDouble() && cellData != null)
                {
                    cellValue.Text = Convert.ToDouble(cellData.ToString()).ToString(CultureInfo.InvariantCulture);
                }
                else if (cellDataType.IsSingle() && cellData != null)
                {
                    cellValue.Text = Convert.ToSingle(cellData.ToString()).ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    cellValue.Text = cellData?.ToString();
                }
            }

            if (cell.DataType == CellValues.Number && !string.IsNullOrEmpty(cellValue.Text))
            {
                cellValue.Text = cellValue.Text.Trim().TrimStart('+');
            }

            if (!string.IsNullOrEmpty(cellValue.Text) && (cellValue.Text.StartsWith('=') || cellValue.Text.StartsWith('@')))
                cellValue.Text = $" ' {cellValue.Text}";

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
        private List<CellFormat>? CellFormats { get; set; }

        public void AddCellFormat(CellFormat format)
        {
            this.CellFormats.Add(format);
        }
        private UInt32 GetStyleIndex(int id)
        {
            var index = this.CellFormats.FindIndex(op => op.NumberFormatId != null && op.NumberFormatId.Value == id);
            if (index == -1) return 0;
            return Convert.ToUInt32(index);
        }
        private void FillCellFormats()
        {
            this.CellFormats = new List<CellFormat>{new CellFormat() { ApplyNumberFormat = false },

                // Numeric formats
                new CellFormat() { NumberFormatId = 1, ApplyNumberFormat = true }, // 0
                new CellFormat() { NumberFormatId = 2, ApplyNumberFormat = true }, // 0.00
                new CellFormat() { NumberFormatId = 3, ApplyNumberFormat = true }, // #,##0
                new CellFormat() { NumberFormatId = 4, ApplyNumberFormat = true }, // #,##0.00
                new CellFormat() { NumberFormatId = 9, ApplyNumberFormat = true }, // %0
                new CellFormat() { NumberFormatId = 10, ApplyNumberFormat = true }, // %0.00
                new CellFormat() { NumberFormatId = 11, ApplyNumberFormat = true }, // 0.00E+00
                new CellFormat() { NumberFormatId = 12, ApplyNumberFormat = true }, // # ?/?
                new CellFormat() { NumberFormatId = 13, ApplyNumberFormat = true }, // # ??/??

                // Date formats
                new CellFormat() { NumberFormatId = 14, ApplyNumberFormat = true }, // YYYY-MM-DD
                new CellFormat() { NumberFormatId = 15, ApplyNumberFormat = true }, // DD/MM/YYYY
                new CellFormat() { NumberFormatId = 16, ApplyNumberFormat = true }, // MM/DD/YYYY
                new CellFormat() { NumberFormatId = 17, ApplyNumberFormat = true }, // DD-MMM-YYYY
                new CellFormat() { NumberFormatId = 18, ApplyNumberFormat = true }, // MMM-YY
                new CellFormat() { NumberFormatId = 19, ApplyNumberFormat = true }, // HH:MM AM/PM
                new CellFormat() { NumberFormatId = 20, ApplyNumberFormat = true }, // h:mm
                new CellFormat() { NumberFormatId = 21, ApplyNumberFormat = true }, // h:mm:ss
                new CellFormat() { NumberFormatId = 22, ApplyNumberFormat = true }, // M/d/yyyy h:mm
                new CellFormat() { NumberFormatId = 45, ApplyNumberFormat = true }, // hh:mm
                new CellFormat() { NumberFormatId = 46, ApplyNumberFormat = true }, // [h]:mm:ss
                new CellFormat() { NumberFormatId = 47, ApplyNumberFormat = true }, // mm:ss
                new CellFormat() { NumberFormatId = 48, ApplyNumberFormat = true }, // [mm]:ss

                // Text Formats
                new CellFormat() { ApplyNumberFormat = false } // Text
            };
        }
        private Stylesheet GenerateStyleSheet()
        {
            return new Stylesheet(
                new Fonts(new Font()),
                new Fills(new Fill()),
                new Borders(new Border()),
                new CellFormats(this.CellFormats)
            );
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