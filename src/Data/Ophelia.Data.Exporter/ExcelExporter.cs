using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Ophelia.Data.Exporter.Controls;
using Ophelia.Data.Exporter.Enums;
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
                string worksheetName = grid.Text.ReplaceSpecialVowelsAndConsonant();

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
                    Name = !string.IsNullOrEmpty(worksheetName) ? worksheetName : $"Sheet{worksheetNumber}"
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
            int numberOfRows = grid.Rows.Count;

            string[] excelColumnNames = new string[numberOfColumns];
            AddHeaderRow(grid, sheetData, excelColumnNames);
            uint rowIndex = 1;
            AddDataRows(grid, sheetData, excelColumnNames, ref rowIndex);

            string startCell = "A1";
            string endCell = GetExcelColumnName(numberOfColumns - 1) + rowIndex.ToString();
            string reference = $"{startCell}:{endCell}";
            string tableStyleInfoName = this.GetTableStyleName(grid.TableStyleID);
            string totalTableStyleInfoName = this.GetTableStyleName(grid.TotalTableStyleID);
            AddTableDefinition(grid, worksheetPart, reference, numberOfColumns, excelColumnNames, tableStyleInfoName);

            if (grid.IsTotalRowShow)
                AddTotalRows(grid, sheetData, excelColumnNames, ref rowIndex, worksheetPart, numberOfColumns, totalTableStyleInfoName);
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
                    {
                        DateTime dateValue;
                        if (cellData is DateTime)
                        {
                            dateValue = (DateTime)cellData;
                            cellValue.Text = dateValue.ToString("s");
                        }
                        else if (cellData is string && DateTime.TryParse(cellData.ToString(), out dateValue) && dateValue > DateTime.MinValue)
                        {
                            cellValue.Text = dateValue.ToString("s");
                        }
                    }
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
                new Fills(new Fill(
    new PatternFill(
        new ForegroundColor { Rgb = new HexBinaryValue() { Value = "FFFFFF00" } }
    )
    { PatternType = PatternValues.Solid }
)),
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
        private void AddHeaderRow(Controls.Grid grid, SheetData sheetData, string[] excelColumnNames)
        {
            for (int n = 0; n < grid.Columns.Count; n++)
                excelColumnNames[n] = GetExcelColumnName(n);

            var headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row { RowIndex = 1 };
            sheetData.Append(headerRow);

            for (int colInx = 0; colInx < grid.Columns.Count; colInx++)
            {
                var column = grid.Columns[colInx];
                AppendCell(
                    excelColumnNames[colInx] + "1",
                    column.Text,
                    headerRow,
                    column.Type.GetValueOrDefault(CellValues.String),
                    null,
                    -1
                );
            }
        }

        private void AddDataRows(Controls.Grid grid, SheetData sheetData, string[] excelColumnNames, ref uint rowIndex)
        {
            foreach (var row in grid.Rows)
            {
                ++rowIndex;
                var newExcelRow = new DocumentFormat.OpenXml.Spreadsheet.Row { RowIndex = rowIndex };
                sheetData.Append(newExcelRow);

                for (int colInx = 0; colInx < grid.Columns.Count; colInx++)
                {
                    var cell = row.Cells[colInx];
                    AppendCell(
                        excelColumnNames[colInx] + rowIndex.ToString(),
                        cell.Value,
                        newExcelRow,
                        cell.Column.Type.GetValueOrDefault(CellValues.String),
                        cell.Column.ValueType,
                        cell.Column.StyleID
                    );
                }
            }
        }
        private void AddTableDefinition(Controls.Grid grid, WorksheetPart worksheetPart, string reference, int numberOfColumns, string[] excelColumnNames, string tableStyleInfoName)
        {
            var worksheet = worksheetPart.Worksheet;

            var tableDefinitionPart = worksheetPart.AddNewPart<TableDefinitionPart>();
            var table = new Table()
            {
                Id = 1U,
                Name = $"MyTable{grid.ID ?? Ophelia.Utility.GenerateRandomPassword(6, true)}",
                DisplayName = $"MyTable{grid.ID ?? Ophelia.Utility.GenerateRandomPassword(6, true)}",
                Reference = reference,
                TotalsRowShown = false
            };

            if (grid.IsFilterable)
                table.AutoFilter = new AutoFilter() { Reference = reference };

            var tableColumns = new TableColumns() { Count = (uint)numberOfColumns };
            for (uint i = 0; i < numberOfColumns; i++)
                tableColumns.Append(new TableColumn() { Id = i + 1, Name = grid.Columns[(int)i].Text });
            table.Append(tableColumns);
            if (!string.IsNullOrEmpty(tableStyleInfoName))
            {
                table.Append(new TableStyleInfo()
                {
                    Name = tableStyleInfoName,
                    ShowFirstColumn = false,
                    ShowLastColumn = false,
                    ShowRowStripes = true,
                    ShowColumnStripes = false
                });

                tableDefinitionPart.Table = table;
                tableDefinitionPart.Table.Save();

                var tableParts = worksheet.Elements<TableParts>().FirstOrDefault();
                if (tableParts == null)
                {
                    tableParts = new TableParts() { Count = 1U };
                    worksheetPart.Worksheet.Append(tableParts);
                }
                else
                {
                    tableParts.Count = (uint)tableParts.ChildElements.Count + 1;
                }
                tableParts.AppendChild(new TablePart() { Id = worksheetPart.GetIdOfPart(tableDefinitionPart) });
                tableParts.Count = (uint)tableParts.ChildElements.Count;
                worksheet.Save();
            }
        }
        private void AddTotalRows(
            Controls.Grid grid,
            SheetData sheetData,
            string[] excelColumnNames,
            ref uint rowIndex,
            WorksheetPart worksheetPart,
            int numberOfColumns,
            string totalTableStyleInfoName)
        {
            ++rowIndex;
            var labelRow = new DocumentFormat.OpenXml.Spreadsheet.Row { RowIndex = rowIndex };
            sheetData.Append(labelRow);
            for (int i = 0; i < numberOfColumns; i++)
            {
                string cellReference = excelColumnNames[i] + rowIndex.ToString();

                var cell = new DocumentFormat.OpenXml.Spreadsheet.Cell()
                {
                    CellReference = cellReference,
                    StyleIndex = (UInt32)grid.Columns[i].Type.GetValueOrDefault(CellValues.String).ToInt32(),
                    DataType = CellValues.String
                };
                cell.Append(new CellValue(i == 0 ? "Toplam" : new string('\u200B', (int)i)));
                labelRow.Append(cell);
            }

            ++rowIndex;
            var totalRow = new DocumentFormat.OpenXml.Spreadsheet.Row { RowIndex = rowIndex };
            sheetData.Append(totalRow);
            for (int i = 0; i < numberOfColumns; i++)
            {
                string cellReference = excelColumnNames[i] + rowIndex.ToString();
                var column = grid.Columns[i];
                var cell = new DocumentFormat.OpenXml.Spreadsheet.Cell()
                {
                    CellReference = cellReference,
                    StyleIndex = (UInt32)column.Type.GetValueOrDefault(CellValues.String).ToInt32()
                };

                if (column.CalculationTypeID > 0)
                {
                    string dataRange = $"{excelColumnNames[i]}2:{excelColumnNames[i]}{rowIndex - 1}";
                    string formula = "";
                    switch ((CalculationType)column.CalculationTypeID)
                    {
                        case CalculationType.Sum:
                            formula = $"SUBTOTAL(109, {dataRange})";
                            break;
                        case CalculationType.Average:
                            formula = $"SUBTOTAL(101, {dataRange})";
                            break;
                        case CalculationType.Count:
                            formula = $"SUBTOTAL(103, {dataRange})";
                            break;
                    }
                    cell.CellFormula = new CellFormula() { Text = formula };
                }
                totalRow.Append(cell);
            }


            string totalStartCell = excelColumnNames[0] + (rowIndex-1).ToString();
            string totalEndCell = excelColumnNames[numberOfColumns - 1] + rowIndex.ToString();
            string totalReference = $"{totalStartCell}:{totalEndCell}";

            var totalTableDefinitionPart = worksheetPart.AddNewPart<TableDefinitionPart>();
            var totalTable = new Table()
            {
                Id = 2U,
                Name = $"TotalTable{grid.ID ?? Ophelia.Utility.GenerateRandomPassword(6, true)}",
                DisplayName = $"TotalTable{grid.ID ?? Ophelia.Utility.GenerateRandomPassword(6, true)}",
                Reference = totalReference,
                TotalsRowShown = false,
            };
            var tableColumns = new TableColumns() { Count = (uint)numberOfColumns };
            for (uint i = 0; i < numberOfColumns; i++)
                tableColumns.Append(new TableColumn() { Id = i + 1, Name = i == 0 ? "Toplam" : new string('\u200B', (int)i) });

            totalTable.Append(tableColumns);

            if (!string.IsNullOrEmpty(totalTableStyleInfoName))
            {
                totalTable.Append(new TableStyleInfo()
                {
                    Name = totalTableStyleInfoName,
                    ShowFirstColumn = false,
                    ShowLastColumn = false,
                    ShowRowStripes = true,
                    ShowColumnStripes = false
                });

                totalTableDefinitionPart.Table = totalTable;
                totalTableDefinitionPart.Table.Save();

                var worksheet = worksheetPart.Worksheet;
                var tableParts = worksheet.Elements<TableParts>().FirstOrDefault();
                if (tableParts == null)
                {
                    tableParts = new TableParts() { Count = 1U };
                    worksheet.Append(tableParts);
                }
                else
                {
                    tableParts.Count = (uint)tableParts.ChildElements.Count + 1;
                }
                tableParts.AppendChild(new TablePart() { Id = worksheetPart.GetIdOfPart(totalTableDefinitionPart) });
                tableParts.Count = (uint)tableParts.ChildElements.Count;
                worksheet.Save();
            }
        }
        private string GetTableStyleName(long tableStyleID = 0)
        {
            var tableStyleInfoName = "";
            switch ((TableStyleType)tableStyleID)
            {
                case TableStyleType.TableStyleLight1:
                    tableStyleInfoName = "TableStyleLight1";
                    break;
                case TableStyleType.TableStyleLight2:
                    tableStyleInfoName = "TableStyleLight2";
                    break;
                case TableStyleType.TableStyleLight3:
                    tableStyleInfoName = "TableStyleLight3";
                    break;
                case TableStyleType.TableStyleLight4:
                    tableStyleInfoName = "TableStyleLight4";
                    break;
                case TableStyleType.TableStyleLight5:
                    tableStyleInfoName = "TableStyleLight5";
                    break;
                case TableStyleType.TableStyleLight6:
                    tableStyleInfoName = "TableStyleLight6";
                    break;
                case TableStyleType.TableStyleLight7:
                    tableStyleInfoName = "TableStyleLight7";
                    break;
                case TableStyleType.TableStyleLight8:
                    tableStyleInfoName = "TableStyleLight8";
                    break;
                case TableStyleType.TableStyleLight9:
                    tableStyleInfoName = "TableStyleLight9";
                    break;
                case TableStyleType.TableStyleLight10:
                    tableStyleInfoName = "TableStyleLight10";
                    break;
                case TableStyleType.TableStyleLight11:
                    tableStyleInfoName = "TableStyleLight11";
                    break;
                case TableStyleType.TableStyleLight12:
                    tableStyleInfoName = "TableStyleLight12";
                    break;
                case TableStyleType.TableStyleLight13:
                    tableStyleInfoName = "TableStyleLight13";
                    break;
                case TableStyleType.TableStyleLight14:
                    tableStyleInfoName = "TableStyleLight14";
                    break;
                case TableStyleType.TableStyleLight15:
                    tableStyleInfoName = "TableStyleLight15";
                    break;
                case TableStyleType.TableStyleLight16:
                    tableStyleInfoName = "TableStyleLight16";
                    break;
                case TableStyleType.TableStyleLight17:
                    tableStyleInfoName = "TableStyleLight17";
                    break;
                case TableStyleType.TableStyleLight18:
                    tableStyleInfoName = "TableStyleLight18";
                    break;
                case TableStyleType.TableStyleLight19:
                    tableStyleInfoName = "TableStyleLight19";
                    break;
                case TableStyleType.TableStyleLight20:
                    tableStyleInfoName = "TableStyleLight20";
                    break;
                case TableStyleType.TableStyleLight21:
                    tableStyleInfoName = "TableStyleLight21";
                    break;
                case TableStyleType.TableStyleMedium1:
                    tableStyleInfoName = "TableStyleMedium1";
                    break;
                case TableStyleType.TableStyleMedium2:
                    tableStyleInfoName = "TableStyleMedium2";
                    break;
                case TableStyleType.TableStyleMedium3:
                    tableStyleInfoName = "TableStyleMedium3";
                    break;
                case TableStyleType.TableStyleMedium4:
                    tableStyleInfoName = "TableStyleMedium4";
                    break;
                case TableStyleType.TableStyleMedium5:
                    tableStyleInfoName = "TableStyleMedium5";
                    break;
                case TableStyleType.TableStyleMedium6:
                    tableStyleInfoName = "TableStyleMedium6";
                    break;
                case TableStyleType.TableStyleMedium7:
                    tableStyleInfoName = "TableStyleMedium7";
                    break;
                case TableStyleType.TableStyleMedium8:
                    tableStyleInfoName = "TableStyleMedium8";
                    break;
                case TableStyleType.TableStyleMedium9:
                    tableStyleInfoName = "TableStyleMedium9";
                    break;
                case TableStyleType.TableStyleMedium10:
                    tableStyleInfoName = "TableStyleMedium10";
                    break;
                case TableStyleType.TableStyleMedium11:
                    tableStyleInfoName = "TableStyleMedium11";
                    break;
                case TableStyleType.TableStyleMedium12:
                    tableStyleInfoName = "TableStyleMedium12";
                    break;
                case TableStyleType.TableStyleMedium13:
                    tableStyleInfoName = "TableStyleMedium13";
                    break;
                case TableStyleType.TableStyleMedium14:
                    tableStyleInfoName = "TableStyleMedium14";
                    break;
                case TableStyleType.TableStyleMedium15:
                    tableStyleInfoName = "TableStyleMedium15";
                    break;
                case TableStyleType.TableStyleMedium16:
                    tableStyleInfoName = "TableStyleMedium16";
                    break;
                case TableStyleType.TableStyleMedium17:
                    tableStyleInfoName = "TableStyleMedium17";
                    break;
                case TableStyleType.TableStyleMedium18:
                    tableStyleInfoName = "TableStyleMedium18";
                    break;
                case TableStyleType.TableStyleMedium19:
                    tableStyleInfoName = "TableStyleMedium19";
                    break;
                case TableStyleType.TableStyleMedium20:
                    tableStyleInfoName = "TableStyleMedium20";
                    break;
                case TableStyleType.TableStyleMedium21:
                    tableStyleInfoName = "TableStyleMedium21";
                    break;
                case TableStyleType.TableStyleMedium22:
                    tableStyleInfoName = "TableStyleMedium22";
                    break;
                case TableStyleType.TableStyleMedium23:
                    tableStyleInfoName = "TableStyleMedium23";
                    break;
                case TableStyleType.TableStyleMedium24:
                    tableStyleInfoName = "TableStyleMedium24";
                    break;
                case TableStyleType.TableStyleMedium25:
                    tableStyleInfoName = "TableStyleMedium25";
                    break;
                case TableStyleType.TableStyleMedium26:
                    tableStyleInfoName = "TableStyleMedium26";
                    break;
                case TableStyleType.TableStyleMedium27:
                    tableStyleInfoName = "TableStyleMedium27";
                    break;
                case TableStyleType.TableStyleMedium28:
                    tableStyleInfoName = "TableStyleMedium28";
                    break;
                case TableStyleType.TableStyleDark1:
                    tableStyleInfoName = "TableStyleDark1";
                    break;
                case TableStyleType.TableStyleDark2:
                    tableStyleInfoName = "TableStyleDark2";
                    break;
                case TableStyleType.TableStyleDark3:
                    tableStyleInfoName = "TableStyleDark3";
                    break;
                case TableStyleType.TableStyleDark4:
                    tableStyleInfoName = "TableStyleDark4";
                    break;
                case TableStyleType.TableStyleDark5:
                    tableStyleInfoName = "TableStyleDark5";
                    break;
                case TableStyleType.TableStyleDark6:
                    tableStyleInfoName = "TableStyleDark6";
                    break;
                case TableStyleType.TableStyleDark7:
                    tableStyleInfoName = "TableStyleDark7";
                    break;
                case TableStyleType.TableStyleDark8:
                    tableStyleInfoName = "TableStyleDark8";
                    break;
                case TableStyleType.TableStyleDark9:
                    tableStyleInfoName = "TableStyleDark9";
                    break;
                case TableStyleType.TableStyleDark10:
                    tableStyleInfoName = "TableStyleDark10";
                    break;
                case TableStyleType.TableStyleDark11:
                    tableStyleInfoName = "TableStyleDark11";
                    break;
                case TableStyleType.None:
                default:
                    tableStyleInfoName = "";
                    break;
            }
            return tableStyleInfoName;
        }

    }
}