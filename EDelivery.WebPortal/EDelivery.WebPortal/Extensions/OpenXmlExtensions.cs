using System;
using System.Globalization;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;

namespace EDelivery.WebPortal.Extensions
{
    public static class OpenXmlExtensions
    {
        // NOTE
        // - rowId - zero based integer
        // - rowIndex - one based integer
        // - columnId - zero based integer
        // - columnIndex - capital letters from A to ZZZ
        // - cellReference - column + row index, i.e. A1, B1, etc.

        private static Regex cellRegex = new Regex("^([A-Z]{1,3})(\\d{1,7})$", RegexOptions.Singleline);
        private static readonly NumberFormatInfo OpenXmlNumbeFormatInfo = new NumberFormatInfo() { NumberDecimalSeparator = ".", NumberGroupSeparator = String.Empty };
        private static readonly Regex InvalidXmlCharsRegex = new Regex("[\x00-\x08\x0B\x0C\x0E-\x1F\x26]");

        public static void InitNormalWorksheet(this WorksheetPart worksheetPart)
        {
            if (worksheetPart.Worksheet != null)
            {
                throw new Exception("Worksheet is already initialized.");
            }

            // NOTE that the order of the elements in the Worksheet is important
            // see https://stackoverflow.com/a/25410242/682203
            worksheetPart.Worksheet = new Worksheet(
                new Columns(),
                new SheetData(),
                new MergeCells(),
                new IgnoredErrors());
        }

        public static void InitPrintableWorksheet(
            this WorksheetPart worksheetPart,
            string headerTitle = null,
            bool gridLines = true)
        {
            if (worksheetPart.Worksheet != null)
            {
                throw new Exception("Worksheet is already initialized.");
            }

            // NOTE that the order of the elements in the Worksheet is important
            // see https://stackoverflow.com/a/25410242/682203
            worksheetPart.Worksheet = new Worksheet(
                new SheetViews(
                    new SheetView() { View = SheetViewValues.PageLayout, WorkbookViewId = 0 }),
                new Columns(),
                new SheetData(),
                new MergeCells(),
                new PrintOptions() { GridLines = gridLines },
                new PageMargins()
                {
                    Left = 0.31D,
                    Right = 0.31D,
                    Top = 0.55D,
                    Bottom = 0.55D,
                    Header = 0.31D,
                    Footer = 0.31D
                },
                new PageSetup()
                {
                    PaperSize = (UInt32Value)9U,
                    Scale = (UInt32Value)100U,
                    PageOrder = PageOrderValues.OverThenDown,
                    Orientation = OrientationValues.Portrait
                },
                new HeaderFooter(
                    new OddHeader(
                        string.IsNullOrEmpty(headerTitle)
                            ? null
                            : $"&L&\"Calibri,Bold\"{headerTitle}"),
                    new OddFooter("&R&P")),
                new RowBreaks(),
                new ColumnBreaks(),
                new IgnoredErrors());
        }

        public static void Finalize(this Worksheet worksheet)
        {
            int maxColumnId = 0;
            int lastRowIndex = 1;
            foreach (Row row in worksheet.GetSheetData())
            {
                var lastCell = row.GetLastCell();

                if (lastCell != null)
                {
                    var (rowIndex, columnIndex) = ParseCellReference(row.GetLastCell().CellReference);
                    int columnId = (int)ColumnIndexToColumnId(columnIndex);
                    maxColumnId = Math.Max(maxColumnId, columnId);

                    lastRowIndex = rowIndex;
                }
            }

            if (maxColumnId != 0 || lastRowIndex != 1)
            {
                // ignore some errors for the entire sheet
                worksheet.GetFirstChild<IgnoredErrors>()
                    .AppendChild(new IgnoredError()
                    {
                        SequenceOfReferences = new ListValue<StringValue>
                        {
                            InnerText = $"A1:{ColumnIdToColumnIndex(maxColumnId)}{lastRowIndex}"
                        },
                        NumberStoredAsText = true,
                        TwoDigitTextYear = true,
                    });
            }

            // there is a posibility that some of these elements are empty
            // leaving an empty element will make Excel show an error
            worksheet.RemoveFirstChildIfEmpty<Columns>();
            worksheet.RemoveFirstChildIfEmpty<MergeCells>();
            worksheet.RemoveFirstChildIfEmpty<IgnoredErrors>();
        }

        private static void RemoveFirstChildIfEmpty<T>(this OpenXmlElement e) where T : OpenXmlElement
        {
            var child = e.GetFirstChild<T>();
            if (child.ChildElements.Count == 0)
            {
                e.RemoveChild(child);
            }
        }

        public static int AppendFont(
            this Stylesheet stylesheet,
            bool bold = false,
            bool italic = false,
            double? size = null,
            string name = null)
        {
            var font = new Font();
            if (bold)
            {
                font.AppendChild(new Bold());
            }
            if (italic)
            {
                font.AppendChild(new Italic());
            }
            if (size.HasValue)
            {
                font.AppendChild(new FontSize() { Val = size });
            }
            if (!string.IsNullOrEmpty(name))
            {
                font.AppendChild(new FontName() { Val = name });
            }

            var fonts = stylesheet.GetFirstChild<Fonts>();
            fonts.AppendChild(font);

            return fonts.ChildElements.Count - 1;
        }

        public static int AppendBorder(
            this Stylesheet stylesheet,
            BorderStyleValues borderStyle)
        {
            var cellBorders = new Border();

            cellBorders.LeftBorder = new LeftBorder(new Color() { Auto = true }) { Style = borderStyle };
            cellBorders.RightBorder = new RightBorder(new Color() { Auto = true }) { Style = borderStyle };
            cellBorders.TopBorder = new TopBorder(new Color() { Auto = true }) { Style = borderStyle };
            cellBorders.BottomBorder = new BottomBorder(new Color() { Auto = true }) { Style = borderStyle };

            var borders = stylesheet.GetFirstChild<Borders>();
            borders.AppendChild(cellBorders);

            return borders.ChildElements.Count - 1;
        }

        public static int AppendCellFormat(
            this Stylesheet stylesheet,
            bool? wrapText = null,
            int? textRotation = null,
            VerticalAlignmentValues? verticalAlignment = null,
            HorizontalAlignmentValues? horizontalAlignment = null,
            int? fontId = null,
            int? numberFormatId = null,
            int? fillId = null,
            int? borderId = null)
        {
            var cellFormat = new CellFormat();

            if (fontId.HasValue)
            {
                cellFormat.FontId = (uint)fontId.Value;
            }

            if (numberFormatId.HasValue)
            {
                cellFormat.NumberFormatId = (uint)numberFormatId.Value;
            }

            if (fillId.HasValue)
            {
                cellFormat.FillId = (uint)fillId.Value;
            }

            if (wrapText != null ||
                textRotation != null ||
                verticalAlignment != null ||
                horizontalAlignment != null ||
                borderId != null)
            {
                var a = cellFormat.AppendChild(new Alignment());

                if (wrapText.HasValue)
                {
                    a.WrapText = wrapText;
                }

                if (textRotation.HasValue)
                {
                    a.TextRotation = (uint)textRotation;
                }

                if (verticalAlignment.HasValue)
                {
                    a.Vertical = new EnumValue<VerticalAlignmentValues>(verticalAlignment);
                }

                if (horizontalAlignment.HasValue)
                {
                    a.Horizontal = new EnumValue<HorizontalAlignmentValues>(horizontalAlignment);
                }

                if (borderId.HasValue)
                {
                    cellFormat.BorderId = (uint)borderId;
                }
            }

            var cellFormats = stylesheet.GetFirstChild<CellFormats>();
            cellFormats.AppendChild(cellFormat);

            return cellFormats.ChildElements.Count - 1;
        }

        public static int AppendFill(this Stylesheet stylesheet, Fill fill)
        {
            var fills = stylesheet.GetFirstChild<Fills>();
            fills.AppendChild(fill);

            return fills.ChildElements.Count - 1;
        }

        public static Columns GetColumns(this Worksheet worksheet)
            => worksheet.GetFirstChild<Columns>();

        public static SheetData GetSheetData(this Worksheet worksheet)
            => worksheet.GetFirstChild<SheetData>();

        public static MergeCells GetMergeCells(this Worksheet worksheet)
            => worksheet.GetFirstChild<MergeCells>();

        public static RowBreaks GetRowBreaks(this Worksheet worksheet)
            => worksheet.GetFirstChild<RowBreaks>();

        public static ColumnBreaks GetColumnBreaks(this Worksheet worksheet)
            => worksheet.GetFirstChild<ColumnBreaks>();

        public static Columns AppendCustomWidthColumn(
            this Columns columns,
            int min,
            int max,
            double width)
        {
            columns.AppendChild(
                new Column()
                {
                    Min = (uint)min,
                    Max = (uint)max,
                    Width = width,
                    CustomWidth = true
                });

            return columns;
        }

        public static Columns AppendCustomWidthColumn(
            this Worksheet worksheet,
            int min,
            int max,
            double width)
            => worksheet.GetColumns().AppendCustomWidthColumn(min, max, width);

        public static Columns AppendRelativeCustomWidthColumn(
            this Columns columns,
            double width,
            int offset = 1,
            int span = 1)
        {
            if (offset < 1)
            {
                throw new Exception($"{nameof(offset)} should not be less than 1");
            }

            if (span < 1)
            {
                throw new Exception($"{nameof(span)} should not be less than 1");
            }

            var lastCol = columns.GetLastChild<Column>();
            var lastColMax = lastCol?.Max ?? lastCol?.Min ?? 0;
            columns.AppendChild(
                new Column()
                {
                    Min = (uint)(lastColMax + offset),
                    Max = (uint)(lastColMax + offset + span - 1),
                    Width = width,
                    CustomWidth = true
                });

            return columns;
        }

        public static Columns AppendRelativeCustomWidthColumn(
            this Worksheet worksheet,
            double width,
            int offset = 1,
            int span = 1)
            => worksheet.GetColumns().AppendRelativeCustomWidthColumn(width, offset, span);

        public static Row GetLastRow(this SheetData sheetData)
            // SheetData should contain only rows and their indexes should be in increasing order
            => (Row)sheetData.LastChild;

        public static Row GetLastRow(this Worksheet worksheet)
            => worksheet.GetSheetData().GetLastRow();

        public static int GetLastRowIndex(this SheetData sheetData)
            => (int)(uint)(sheetData.GetLastRow()?.RowIndex ?? 0);

        public static int GetLastRowIndex(this Worksheet worksheet)
            => worksheet.GetSheetData().GetLastRowIndex();

        public static T GetLastChild<T>(this OpenXmlElement e) where T : OpenXmlElement
        {
            var lastChild = e.LastChild;
            while (lastChild != null && !(lastChild is T))
            {
                lastChild = lastChild.PreviousSibling();
            }

            return lastChild as T;
        }

        public static Cell GetLastCell(this Row row)
            => row.GetLastChild<Cell>();

        public static Row AppendRow(this SheetData sheetData, int rowIndex, double? height = null)
        {
            var row = sheetData.AppendChild(new Row() { RowIndex = (uint)rowIndex });

            if (height.HasValue)
            {
                row.CustomHeight = true;
                row.Height = height.Value;
            }

            return row;
        }

        public static Row AppendRow(this Worksheet worksheet, int rowIndex, double? height = null)
            => worksheet.GetSheetData().AppendRow(rowIndex, height);

        public static Row AppendRelativeRow(this SheetData sheetData, int offset = 1, double? height = null)
        {
            if (offset < 1)
            {
                throw new Exception($"{nameof(offset)} should not be less than 1");
            }

            return sheetData.AppendRow(sheetData.GetLastRowIndex() + offset, height);
        }

        public static Row AppendRelativeRow(this Worksheet worksheet, int offset = 1, double? height = null)
            => worksheet.GetSheetData().AppendRelativeRow(offset, height);

        public static Row AppendInlineStringCell(
            this Row row,
            string columnIndex,
            OpenXmlElement[] childElements,
            int? styleIndex = null)
        {
            var cell = row.AppendChild(
                new Cell(new InlineString(childElements))
                {
                    CellReference = $"{columnIndex}{row.RowIndex}",
                    DataType = CellValues.InlineString,
                });

            if (styleIndex.HasValue)
            {
                cell.StyleIndex = (uint)styleIndex.Value;
            }

            return row;
        }

        public static Row AppendInlineStringCell(
            this Row row,
            string columnIndex,
            string text,
            int? styleIndex = null)
        {
            text = RemoveInvalidXmlChars(text);

            return row.AppendInlineStringCell(
                columnIndex,
                new OpenXmlElement[] { new Text(text) },
                styleIndex);
        }

        private static Row AppendTypedCell(
            this Row row,
            string columnIndex,
            string value,
            CellValues dataType,
            int? styleIndex)
        {
            value = RemoveInvalidXmlChars(value);

            var cell = row.AppendChild(
                new Cell()
                {
                    CellValue = new CellValue(value),
                    CellReference = $"{columnIndex}{row.RowIndex}",
                    DataType = dataType,
                });

            if (styleIndex.HasValue)
            {
                cell.StyleIndex = (uint)styleIndex.Value;
            }

            return row;
        }

        public static Row AppendNumberCell(
            this Row row,
            string columnIndex,
            int? number,
            int? styleIndex = null)
            => row.AppendTypedCell(columnIndex, number?.ToString() ?? "", CellValues.Number, styleIndex);

        public static Row AppendNumberCell(
            this Row row,
            string columnIndex,
            decimal? number,
            int? styleIndex = null)
            => row.AppendTypedCell(columnIndex, number?.ToString("0.00", OpenXmlNumbeFormatInfo) ?? "", CellValues.Number, styleIndex);

        public static Row AppendNumberCell(
            this Row row,
            string columnIndex,
            double? number,
            int? styleIndex = null)
            => row.AppendTypedCell(columnIndex, number?.ToString("0.00", OpenXmlNumbeFormatInfo) ?? "", CellValues.Number, styleIndex);

        public static Row AppendDateCell(
            this Row row,
            string columnIndex,
            DateTime? date,
            int? styleIndex = null)
            // see https://stackoverflow.com/a/39629492/682203
            => row.AppendTypedCell(columnIndex, (date?.ToOADate())?.ToString(CultureInfo.InvariantCulture) ?? "", CellValues.Number, styleIndex);

        private static string GetRelativeCellColumnIndex(this Row row, int offset)
        {
            if (offset < 1)
            {
                throw new Exception($"{nameof(offset)} should not be less than 1");
            }

            // the row has mixed children so we cant just use LastChild property
            var lastCell = row.GetLastChild<Cell>();

            int columnId;
            if (lastCell != null)
            {
                var (rowIndex, columnIndex) = ParseCellReference(lastCell.CellReference);
                columnId = (int)ColumnIndexToColumnId(columnIndex);
            }
            else
            {
                columnId = -1;
            }

            return ColumnIdToColumnIndex(columnId + offset);
        }

        public static Row AppendRelativeInlineStringCell(
            this Row row,
            int offset = 1,
            string text = "",
            int? styleIndex = null)
            => row.AppendInlineStringCell(row.GetRelativeCellColumnIndex(offset), text, styleIndex);

        public static Row AppendRelativeNumberCell(
            this Row row,
            int offset = 1,
            int? number = null,
            int? styleIndex = null)
            => row.AppendNumberCell(row.GetRelativeCellColumnIndex(offset), number, styleIndex);

        public static Row AppendRelativeNumberCell(
            this Row row,
            int offset = 1,
            decimal? number = null,
            int? styleIndex = null)
            => row.AppendNumberCell(row.GetRelativeCellColumnIndex(offset), number, styleIndex);

        public static Row AppendRelativeNumberCell(
            this Row row,
            int offset = 1,
            double? number = null,
            int? styleIndex = null)
            => row.AppendNumberCell(row.GetRelativeCellColumnIndex(offset), number, styleIndex);

        public static Row AppendRelativeDateCell(
            this Row row,
            int offset = 1,
            DateTime? date = null,
            int? styleIndex = null)
            => row.AppendDateCell(row.GetRelativeCellColumnIndex(offset), date, styleIndex);

        public static MergeCells AppendMergeCell(this MergeCells mergeCells, string reference)
        {
            mergeCells.AppendChild(new MergeCell() { Reference = reference });
            return mergeCells;
        }

        public static MergeCells AppendMergeCell(this Worksheet worksheet, string reference)
            => worksheet.GetMergeCells().AppendMergeCell(reference);

        public static MergeCells AppendMergeCell(this MergeCells mergeCells, string cellReference, int rows = 1, int cols = 1)
        {
            if (rows < 1 || cols < 1)
            {
                throw new Exception($"{nameof(rows)} and {nameof(cols)} should not be less than 1");
            }

            if (rows + cols <= 2)
            {
                throw new Exception($"{nameof(rows)} + {nameof(cols)} should not be less than or equal to 2");
            }

            var (rowIndex, columnIndex) = ParseCellReference(cellReference);
            var columnId = ColumnIndexToColumnId(columnIndex);

            var start = cellReference;
            var end = $"{ColumnIdToColumnIndex(columnId + cols - 1)}{rowIndex + rows - 1}";

            mergeCells.AppendMergeCell($"{start}:{end}");
            return mergeCells;
        }

        public static MergeCells AppendMergeCell(this Worksheet worksheet, string cellReference, int rows = 1, int cols = 1)
            => worksheet.GetMergeCells().AppendMergeCell(cellReference, rows, cols);

        public static Worksheet AppendRelativeMergeCell(this Worksheet worksheet, int rows = 1, int cols = 1, Row relativeRow = null)
        {
            if (rows < 1 || cols < 1)
            {
                throw new Exception($"{nameof(rows)} and {nameof(cols)} should not be less than 1");
            }

            if (rows + cols <= 2)
            {
                throw new Exception($"{nameof(rows)} + {nameof(cols)} should not be less than or equal to 2");
            }

            var lastRow = relativeRow ?? worksheet.GetLastRow();
            if (lastRow == null)
            {
                throw new Exception("SheetData has no rows");
            }

            // the row has mixed children so we cant just use LastChild property
            var lastCell = lastRow.GetLastChild<Cell>();

            if (lastCell == null)
            {
                throw new Exception("No LastCell found. To use the relative MergeCell methods at least one cell should have been added to the last row.");
            }

            worksheet.AppendMergeCell(lastCell.CellReference, rows, cols);
            return worksheet;
        }

        public static RowBreaks AppendRowBreak(this RowBreaks rowBreaks, int beforeRowIndex)
        {
            rowBreaks.AppendChild(new Break() { Id = (uint)beforeRowIndex - 1, Max = 16383U, ManualPageBreak = true });

            rowBreaks.Count = (uint)rowBreaks.ChildElements.Count;
            rowBreaks.ManualBreakCount = (uint)rowBreaks.ChildElements.Count;

            return rowBreaks;
        }

        public static RowBreaks AppendRowBreak(this Worksheet worksheet, int beforeRowIndex)
            => worksheet.GetRowBreaks().AppendRowBreak(beforeRowIndex);

        public static RowBreaks AppendRelativeRowBreak(this Worksheet worksheet, int offset = 1)
        {
            if (offset < 1)
            {
                throw new Exception($"{nameof(offset)} should not be less than 1");
            }

            return worksheet.AppendRowBreak(worksheet.GetLastRowIndex() + offset);
        }

        public static ColumnBreaks AppendColumnBreak(this ColumnBreaks columnBreaks, int beforeColumnId)
        {
            columnBreaks.AppendChild(new Break() { Id = (uint)beforeColumnId, Max = 16383U, ManualPageBreak = true });

            columnBreaks.Count = (uint)columnBreaks.ChildElements.Count;
            columnBreaks.ManualBreakCount = (uint)columnBreaks.ChildElements.Count;

            return columnBreaks;
        }

        public static ColumnBreaks AppendColumnBreak(this Worksheet worksheet, int beforeColumnId)
            => worksheet.GetColumnBreaks().AppendColumnBreak(beforeColumnId);

        public static ColumnBreaks AppendColumnBreak(this Worksheet worksheet, string beforeColumnIndex)
            => worksheet.GetColumnBreaks().AppendColumnBreak(ColumnIndexToColumnId(beforeColumnIndex));

        public static ColumnBreaks AppendRelativeColumnBreak(this Worksheet worksheet, int offset = 1)
        {
            if (offset < 1)
            {
                throw new Exception($"{nameof(offset)} should not be less than 1");
            }

            var lastCell = worksheet.GetLastRow().GetLastChild<Cell>();
            var (rowIndex, columnIndex) = ParseCellReference(lastCell.CellReference);
            var columnId = ColumnIndexToColumnId(columnIndex);

            return worksheet.AppendColumnBreak(columnId + offset);
        }

        private static string RemoveInvalidXmlChars(string text)
        {
            if (text != null && InvalidXmlCharsRegex.IsMatch(text))
            {
                return InvalidXmlCharsRegex.Replace(text, string.Empty);
            }

            return text;
        }

        public static string ColumnIdToColumnIndex(int columnId)
        {
            if (columnId >= 0 && columnId <= 25)
            {
                return ((char)(((int)'A') + columnId)).ToString();
            }

            if (columnId >= 26 && columnId <= 701)
            {
                int v = columnId - 26;
                int h = v / 26;
                int l = v % 26;
                return ((char)(((int)'A') + h)).ToString() + ((char)(((int)'A') + l)).ToString();
            }

            // 17576
            if (columnId >= 702 && columnId <= 18277)
            {
                int v = columnId - 702;
                int h = v / 676;
                int r = v % 676;
                int m = r / 26;
                int l = r % 26;
                return ((char)(((int)'A') + h)).ToString() +
                    ((char)(((int)'A') + m)).ToString() +
                    ((char)(((int)'A') + l)).ToString();
            }

            throw new Exception($"ColumnId out of range ${columnId.ToString()}");
        }

        public static int ColumnIndexToColumnId(string col)
        {
            if (col.Length > 3)
            {
                throw new Exception("Column out of range ");
            }

            int colIndex = 0;
            int[] coef = { 1, 26, 26 * 26 };
            for (int i = 0; i < col.Length; i++)
            {
                colIndex += coef[col.Length - i - 1] * ((int)col[i] - (int)'A' + 1);
            }

            return colIndex - 1;
        }

        public static (int rowIndex, string columnIndex) ParseCellReference(string cellReference)
        {
            var m = cellRegex.Match(cellReference);
            if (!m.Success)
            {
                throw new Exception($"Invalid cellReference '{cellReference}'.");
            }
            return (rowIndex: int.Parse(m.Groups[2].Value), columnIndex: m.Groups[1].Value);
        }
    }
}
