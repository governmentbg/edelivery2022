﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;

using ED.DomainServices.Blobs;
using ED.DomainServices.Messages;

using EDelivery.WebPortal.Extensions;
using EDelivery.WebPortal.Models;

using Microsoft.AspNet.Identity;

namespace EDelivery.WebPortal
{
    public class ExportService
    {
        public static FileStreamResult ExportInbox(InboxResponse data)
        {
            DateTime now = DateTime.Now;

            MemoryStream excelStream = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(excelStream, SpreadsheetDocumentType.Workbook, autoSave: true))
            {
                WorkbookPart workbookPart = document.AddNewPart<WorkbookPart>(
                    @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml",
                    "workbook");

                WorkbookStylesPart workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>("styles");
                workbookStylesPart.Stylesheet = new Stylesheet(
                    new NumberingFormats(
                        new NumberingFormat()
                        {
                            NumberFormatId = 165,
                            FormatCode = @"[$]dd\.mm\.yy;@",
                        }
                    ),
                    new Fonts(
                        new Font() // Default style
                    ),
                    new Fills(
                        new Fill() // Default fill
                    ),
                    new Borders(
                        new Border() // Default border
                    ),
                    new CellFormats(
                        new CellFormat() // Default cell format
                    ));

                workbookPart.Workbook = new Workbook(new Sheets());

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>("Sheet1");

                Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
                Sheet lastSheet = sheets.GetLastChild<Sheet>();

                sheets.AppendChild(
                    new Sheet()
                    {
                        Id = workbookPart.GetIdOfPart(worksheetPart),
                        SheetId = (lastSheet?.SheetId?.Value ?? 0) + 1,
                        Name = "Резултат",
                    });

                worksheetPart.InitNormalWorksheet();

                int titleFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 14.0);
                int headerFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 12.0);

                int titleTableCellFormat = workbookStylesPart.Stylesheet.AppendCellFormat(
                    verticalAlignment: VerticalAlignmentValues.Bottom,
                    horizontalAlignment: HorizontalAlignmentValues.Left,
                    wrapText: true,
                    fontId: titleFont);

                int headerTableCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                    verticalAlignment: VerticalAlignmentValues.Center,
                    horizontalAlignment: HorizontalAlignmentValues.Center,
                    wrapText: true,
                    fontId: headerFont);

                int dateCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                    // special date number format added in the Stylesheet
                    numberFormatId: 165);

                string[] headerRowColumnTitles = new string[]
                {
                    "Уникален №",
                    "Шаблон",
                    "Профил подател",
                    "Заглавие",
                    "Дата на изпращане",
                    "Дата на получаване"
                };

                Row titleRow = worksheetPart.Worksheet.AppendRelativeRow();

                titleRow.AppendRelativeInlineStringCell(
                    text: $"Експорт на първите {SystemConstants.ExportSize} резултата от списък Получени съобщения към дата {now.ToString(SystemConstants.DateTimeFormat)}",
                    styleIndex: titleTableCellFormat);
                worksheetPart.Worksheet
                    .AppendMergeCell($"A{titleRow.RowIndex}:" +
                        $"{OpenXmlExtensions.ColumnIdToColumnIndex(headerRowColumnTitles.Length - 1)}{titleRow.RowIndex}");

                double primaryColWidth = 13;
                worksheetPart.Worksheet.GetColumns()
                    .AppendCustomWidthColumn(1, 1, primaryColWidth)
                    .AppendCustomWidthColumn(2, 2, primaryColWidth)
                    .AppendCustomWidthColumn(3, 3, primaryColWidth * 4)
                    .AppendCustomWidthColumn(4, 4, primaryColWidth * 4)
                    .AppendCustomWidthColumn(5, 5, primaryColWidth)
                    .AppendCustomWidthColumn(6, 6, primaryColWidth);

                Row headerRow = worksheetPart.Worksheet.AppendRelativeRow();
                foreach (string columnTitle in headerRowColumnTitles)
                {
                    headerRow
                        .AppendRelativeInlineStringCell(
                            text: $"{columnTitle}",
                            styleIndex: headerTableCell);
                }

                foreach (var message in data.Result)
                {
                    Row messageRow = worksheetPart.Worksheet.AppendRelativeRow();

                    messageRow.AppendRelativeNumberCell(
                        number: message.MessageId);

                    messageRow.AppendRelativeInlineStringCell(
                        text: message.TemplateName);

                    messageRow.AppendRelativeInlineStringCell(
                        text: message.SenderProfileName);

                    messageRow.AppendRelativeInlineStringCell(
                        text: message.Subject);

                    messageRow.AppendRelativeDateCell(
                        date: message.DateSent.ToLocalDateTime(),
                        styleIndex: dateCell);

                    messageRow.AppendRelativeDateCell(
                        date: message.DateReceived?.ToLocalDateTime(),
                        styleIndex: dateCell);
                }

                worksheetPart.Worksheet.Finalize();
                document.Close();

                excelStream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"inbox_{now:yyyyMMdd}.xlsx"
                };
            }
        }

        public static FileStreamResult ExportOutbox(OutboxResponse data)
        {
            DateTime now = DateTime.Now;

            MemoryStream excelStream = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(excelStream, SpreadsheetDocumentType.Workbook, autoSave: true))
            {
                WorkbookPart workbookPart = document.AddNewPart<WorkbookPart>(
                    @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml",
                    "workbook");

                WorkbookStylesPart workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>("styles");
                workbookStylesPart.Stylesheet = new Stylesheet(
                    new NumberingFormats(
                        new NumberingFormat()
                        {
                            NumberFormatId = 165,
                            FormatCode = @"[$]dd\.mm\.yy;@",
                        }
                    ),
                    new Fonts(
                        new Font() // Default style
                    ),
                    new Fills(
                        new Fill() // Default fill
                    ),
                    new Borders(
                        new Border() // Default border
                    ),
                    new CellFormats(
                        new CellFormat() // Default cell format
                    ));

                workbookPart.Workbook = new Workbook(new Sheets());

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>("Sheet1");

                Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
                Sheet lastSheet = sheets.GetLastChild<Sheet>();

                sheets.AppendChild(
                    new Sheet()
                    {
                        Id = workbookPart.GetIdOfPart(worksheetPart),
                        SheetId = (lastSheet?.SheetId?.Value ?? 0) + 1,
                        Name = "Резултат",
                    });

                worksheetPart.InitNormalWorksheet();

                int titleFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 14.0);
                int headerFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 12.0);

                int titleTableCellFormat = workbookStylesPart.Stylesheet.AppendCellFormat(
                    verticalAlignment: VerticalAlignmentValues.Bottom,
                    horizontalAlignment: HorizontalAlignmentValues.Left,
                    wrapText: true,
                    fontId: titleFont);

                int headerTableCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                    verticalAlignment: VerticalAlignmentValues.Center,
                    horizontalAlignment: HorizontalAlignmentValues.Center,
                    wrapText: true,
                    fontId: headerFont);

                int dateCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                    // special date number format added in the Stylesheet
                    numberFormatId: 165);

                string[] headerRowColumnTitles = new string[]
                {
                    "Уникален №",
                    "Шаблон",
                    "Профили получатели",
                    "Заглавие",
                    "Дата на изпращане",
                    "Брой получени"
                };

                Row titleRow = worksheetPart.Worksheet.AppendRelativeRow();

                titleRow.AppendRelativeInlineStringCell(
                    text: $"Експорт на първите {SystemConstants.ExportSize} резултата от списък Изпратени съобщения към дата {now.ToString(SystemConstants.DateTimeFormat)}",
                    styleIndex: titleTableCellFormat);
                worksheetPart.Worksheet
                    .AppendMergeCell($"A{titleRow.RowIndex}:" +
                        $"{OpenXmlExtensions.ColumnIdToColumnIndex(headerRowColumnTitles.Length - 1)}{titleRow.RowIndex}");

                double primaryColWidth = 13;
                worksheetPart.Worksheet.GetColumns()
                    .AppendCustomWidthColumn(1, 1, primaryColWidth)
                    .AppendCustomWidthColumn(2, 2, primaryColWidth)
                    .AppendCustomWidthColumn(3, 3, primaryColWidth * 4)
                    .AppendCustomWidthColumn(4, 4, primaryColWidth * 4)
                    .AppendCustomWidthColumn(5, 5, primaryColWidth)
                    .AppendCustomWidthColumn(6, 6, primaryColWidth);

                Row headerRow = worksheetPart.Worksheet.AppendRelativeRow();
                foreach (string columnTitle in headerRowColumnTitles)
                {
                    headerRow
                        .AppendRelativeInlineStringCell(
                            text: $"{columnTitle}",
                            styleIndex: headerTableCell);
                }

                foreach (var message in data.Result)
                {
                    Row messageRow = worksheetPart.Worksheet.AppendRelativeRow();

                    messageRow.AppendRelativeNumberCell(
                        number: message.MessageId);

                    messageRow.AppendRelativeInlineStringCell(
                        text: message.TemplateName);

                    messageRow.AppendRelativeInlineStringCell(
                        text: message.Recipients);

                    messageRow.AppendRelativeInlineStringCell(
                        text: message.Subject);

                    messageRow.AppendRelativeDateCell(
                        date: message.DateSent.ToLocalDateTime(),
                        styleIndex: dateCell);

                    messageRow.AppendRelativeInlineStringCell(
                        text: $"{message.NumberOfRecipients}/{message.NumberOfTotalRecipients}");
                }

                worksheetPart.Worksheet.Finalize();
                document.Close();

                excelStream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"outbox_{now:yyyyMMdd}.xlsx"
                };
            }
        }

        public static FileStreamResult ExportHistory(List<ProfileHistoryRecord> data)
        {
            DateTime now = DateTime.Now;

            MemoryStream excelStream = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(excelStream, SpreadsheetDocumentType.Workbook, autoSave: true))
            {
                WorkbookPart workbookPart = document.AddNewPart<WorkbookPart>(
                    @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml",
                    "workbook");

                WorkbookStylesPart workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>("styles");
                workbookStylesPart.Stylesheet = new Stylesheet(
                    new NumberingFormats(
                        new NumberingFormat()
                        {
                            NumberFormatId = 165,
                            FormatCode = @"[$]dd\.mm\.yy;@",
                        }
                    ),
                    new Fonts(
                        new Font() // Default style
                    ),
                    new Fills(
                        new Fill() // Default fill
                    ),
                    new Borders(
                        new Border() // Default border
                    ),
                    new CellFormats(
                        new CellFormat() // Default cell format
                    ));

                workbookPart.Workbook = new Workbook(new Sheets());

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>("Sheet1");

                Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
                Sheet lastSheet = sheets.GetLastChild<Sheet>();

                sheets.AppendChild(
                    new Sheet()
                    {
                        Id = workbookPart.GetIdOfPart(worksheetPart),
                        SheetId = (lastSheet?.SheetId?.Value ?? 0) + 1,
                        Name = "Резултат",
                    });

                worksheetPart.InitNormalWorksheet();

                int titleFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 14.0);
                int headerFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 12.0);

                int titleTableCellFormat = workbookStylesPart.Stylesheet.AppendCellFormat(
                    verticalAlignment: VerticalAlignmentValues.Bottom,
                    horizontalAlignment: HorizontalAlignmentValues.Left,
                    wrapText: true,
                    fontId: titleFont);

                int headerTableCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                    verticalAlignment: VerticalAlignmentValues.Center,
                    horizontalAlignment: HorizontalAlignmentValues.Center,
                    wrapText: true,
                    fontId: headerFont);

                int dateCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                    // special date number format added in the Stylesheet
                    numberFormatId: 165);

                string[] headerRowColumnTitles = new string[]
                {
                    "Дата",
                    "Действие",
                    "Извършено от",
                    "IP адрес",
                    "Детайли",
                };

                Row titleRow = worksheetPart.Worksheet.AppendRelativeRow();

                titleRow.AppendRelativeInlineStringCell(
                    text: $"Експорт на първите {SystemConstants.ExportSize} резултата от списък История на профил към дата {now.ToString(SystemConstants.DateTimeFormat)}",
                    styleIndex: titleTableCellFormat);
                worksheetPart.Worksheet
                    .AppendMergeCell($"A{titleRow.RowIndex}:" +
                        $"{OpenXmlExtensions.ColumnIdToColumnIndex(headerRowColumnTitles.Length - 1)}{titleRow.RowIndex}");

                double primaryColWidth = 13;
                worksheetPart.Worksheet.GetColumns()
                    .AppendCustomWidthColumn(1, 1, primaryColWidth)
                    .AppendCustomWidthColumn(2, 2, primaryColWidth)
                    .AppendCustomWidthColumn(3, 3, primaryColWidth * 4)
                    .AppendCustomWidthColumn(4, 4, primaryColWidth)
                    .AppendCustomWidthColumn(5, 5, primaryColWidth * 4);

                Row headerRow = worksheetPart.Worksheet.AppendRelativeRow();
                foreach (string columnTitle in headerRowColumnTitles)
                {
                    headerRow
                        .AppendRelativeInlineStringCell(
                            text: $"{columnTitle}",
                            styleIndex: headerTableCell);
                }

                foreach (var record in data)
                {
                    Row messageRow = worksheetPart.Worksheet.AppendRelativeRow();

                    messageRow.AppendRelativeDateCell(
                       date: record.Date,
                       styleIndex: dateCell);

                    messageRow.AppendRelativeInlineStringCell(
                        text: record.Action);

                    messageRow.AppendRelativeInlineStringCell(
                        text: record.UserName);

                    messageRow.AppendRelativeInlineStringCell(
                        text: record.Ip);

                    messageRow.AppendRelativeInlineStringCell(
                        text: record.Details);
                }

                worksheetPart.Worksheet.Finalize();
                document.Close();

                excelStream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"profile_history_{now:yyyyMMdd}.xlsx"
                };
            }
        }

        public static FileStreamResult ExportFreeBlobs(GetProfileFreeBlobsResponse data)
        {
            DateTime now = DateTime.Now;

            MemoryStream excelStream = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(excelStream, SpreadsheetDocumentType.Workbook, autoSave: true))
            {
                WorkbookPart workbookPart = document.AddNewPart<WorkbookPart>(
                    @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml",
                    "workbook");

                WorkbookStylesPart workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>("styles");
                workbookStylesPart.Stylesheet = new Stylesheet(
                    new NumberingFormats(
                        new NumberingFormat()
                        {
                            NumberFormatId = 165,
                            FormatCode = @"[$]dd\.mm\.yy;@",
                        }
                    ),
                    new Fonts(
                        new Font() // Default style
                    ),
                    new Fills(
                        new Fill() // Default fill
                    ),
                    new Borders(
                        new Border() // Default border
                    ),
                    new CellFormats(
                        new CellFormat() // Default cell format
                    ));

                workbookPart.Workbook = new Workbook(new Sheets());

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>("Sheet1");

                Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
                Sheet lastSheet = sheets.GetLastChild<Sheet>();

                sheets.AppendChild(
                    new Sheet()
                    {
                        Id = workbookPart.GetIdOfPart(worksheetPart),
                        SheetId = (lastSheet?.SheetId?.Value ?? 0) + 1,
                        Name = "Резултат",
                    });

                worksheetPart.InitNormalWorksheet();

                int titleFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 14.0);
                int headerFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 12.0);

                int titleTableCellFormat = workbookStylesPart.Stylesheet.AppendCellFormat(
                    verticalAlignment: VerticalAlignmentValues.Bottom,
                    horizontalAlignment: HorizontalAlignmentValues.Left,
                    wrapText: true,
                    fontId: titleFont);

                int headerTableCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                    verticalAlignment: VerticalAlignmentValues.Center,
                    horizontalAlignment: HorizontalAlignmentValues.Center,
                    wrapText: true,
                    fontId: headerFont);

                int dateCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                    // special date number format added in the Stylesheet
                    numberFormatId: 165);

                string[] headerRowColumnTitles = new string[]
                {
                    "Уникален №",
                    "Име на файл",
                    "Дата",
                    "Размер",
                };

                Row titleRow = worksheetPart.Worksheet.AppendRelativeRow();

                titleRow.AppendRelativeInlineStringCell(
                    text: $"Експорт на първите {SystemConstants.ExportSize} резултата от списък Хранилище към дата {now.ToString(SystemConstants.DateTimeFormat)}",
                    styleIndex: titleTableCellFormat);
                worksheetPart.Worksheet
                    .AppendMergeCell($"A{titleRow.RowIndex}:" +
                        $"{OpenXmlExtensions.ColumnIdToColumnIndex(headerRowColumnTitles.Length - 1)}{titleRow.RowIndex}");

                double primaryColWidth = 13;
                worksheetPart.Worksheet.GetColumns()
                    .AppendCustomWidthColumn(1, 1, primaryColWidth)
                    .AppendCustomWidthColumn(2, 2, primaryColWidth * 4)
                    .AppendCustomWidthColumn(3, 3, primaryColWidth)
                    .AppendCustomWidthColumn(4, 4, primaryColWidth);

                Row headerRow = worksheetPart.Worksheet.AppendRelativeRow();
                foreach (string columnTitle in headerRowColumnTitles)
                {
                    headerRow
                        .AppendRelativeInlineStringCell(
                            text: $"{columnTitle}",
                            styleIndex: headerTableCell);
                }

                foreach (var blob in data.Result)
                {
                    Row blobRow = worksheetPart.Worksheet.AppendRelativeRow();

                    blobRow.AppendRelativeNumberCell(
                        number: blob.BlobId);

                    blobRow.AppendRelativeInlineStringCell(
                        text: blob.FileName);

                    blobRow.AppendRelativeDateCell(
                        date: blob.CreateDate.ToLocalDateTime(),
                        styleIndex: dateCell);

                    blobRow.AppendRelativeInlineStringCell(
                        text: Utils.Utils.FormatSize(Convert.ToUInt64(blob.Size)));
                }

                worksheetPart.Worksheet.Finalize();
                document.Close();

                excelStream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"storage_free_{now:yyyyMMdd}.xlsx"
                };
            }
        }

        public static FileStreamResult ExportInboxBlobs(GetProfileInboxBlobsResponse data)
        {
            DateTime now = DateTime.Now;

            MemoryStream excelStream = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(excelStream, SpreadsheetDocumentType.Workbook, autoSave: true))
            {
                WorkbookPart workbookPart = document.AddNewPart<WorkbookPart>(
                    @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml",
                    "workbook");

                WorkbookStylesPart workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>("styles");
                workbookStylesPart.Stylesheet = new Stylesheet(
                    new NumberingFormats(
                        new NumberingFormat()
                        {
                            NumberFormatId = 165,
                            FormatCode = @"[$]dd\.mm\.yy;@",
                        }
                    ),
                    new Fonts(
                        new Font() // Default style
                    ),
                    new Fills(
                        new Fill() // Default fill
                    ),
                    new Borders(
                        new Border() // Default border
                    ),
                    new CellFormats(
                        new CellFormat() // Default cell format
                    ));

                workbookPart.Workbook = new Workbook(new Sheets());

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>("Sheet1");

                Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
                Sheet lastSheet = sheets.GetLastChild<Sheet>();

                sheets.AppendChild(
                    new Sheet()
                    {
                        Id = workbookPart.GetIdOfPart(worksheetPart),
                        SheetId = (lastSheet?.SheetId?.Value ?? 0) + 1,
                        Name = "Резултат",
                    });

                worksheetPart.InitNormalWorksheet();

                int titleFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 14.0);
                int headerFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 12.0);

                int titleTableCellFormat = workbookStylesPart.Stylesheet.AppendCellFormat(
                    verticalAlignment: VerticalAlignmentValues.Bottom,
                    horizontalAlignment: HorizontalAlignmentValues.Left,
                    wrapText: true,
                    fontId: titleFont);

                int headerTableCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                    verticalAlignment: VerticalAlignmentValues.Center,
                    horizontalAlignment: HorizontalAlignmentValues.Center,
                    wrapText: true,
                    fontId: headerFont);

                int dateCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                    // special date number format added in the Stylesheet
                    numberFormatId: 165);

                string[] headerRowColumnTitles = new string[]
                {
                    "Уникален №",
                    "Име на файл",
                    "Дата",
                    "Размер",
                    "Съобщение №",
                    "Съобщение",
                };

                Row titleRow = worksheetPart.Worksheet.AppendRelativeRow();

                titleRow.AppendRelativeInlineStringCell(
                    text: $"Експорт на първите {SystemConstants.ExportSize} резултата от списък Хранилище (Получени) към дата {now.ToString(SystemConstants.DateTimeFormat)}",
                    styleIndex: titleTableCellFormat);
                worksheetPart.Worksheet
                    .AppendMergeCell($"A{titleRow.RowIndex}:" +
                        $"{OpenXmlExtensions.ColumnIdToColumnIndex(headerRowColumnTitles.Length - 1)}{titleRow.RowIndex}");

                double primaryColWidth = 13;
                worksheetPart.Worksheet.GetColumns()
                    .AppendCustomWidthColumn(1, 1, primaryColWidth)
                    .AppendCustomWidthColumn(2, 2, primaryColWidth * 4)
                    .AppendCustomWidthColumn(3, 3, primaryColWidth)
                    .AppendCustomWidthColumn(4, 4, primaryColWidth)
                    .AppendCustomWidthColumn(5, 5, primaryColWidth)
                    .AppendCustomWidthColumn(6, 6, primaryColWidth * 4);

                Row headerRow = worksheetPart.Worksheet.AppendRelativeRow();
                foreach (string columnTitle in headerRowColumnTitles)
                {
                    headerRow
                        .AppendRelativeInlineStringCell(
                            text: $"{columnTitle}",
                            styleIndex: headerTableCell);
                }

                foreach (var blob in data.Result)
                {
                    Row blobRow = worksheetPart.Worksheet.AppendRelativeRow();

                    blobRow.AppendRelativeNumberCell(
                        number: blob.BlobId);

                    blobRow.AppendRelativeInlineStringCell(
                        text: blob.FileName);

                    blobRow.AppendRelativeDateCell(
                        date: blob.CreateDate.ToLocalDateTime(),
                        styleIndex: dateCell);

                    blobRow.AppendRelativeInlineStringCell(
                        text: Utils.Utils.FormatSize(Convert.ToUInt64(blob.Size)));

                    blobRow.AppendRelativeNumberCell(
                        number: blob.MessageId);

                    blobRow.AppendRelativeInlineStringCell(
                        text: blob.MessageSubject);
                }

                worksheetPart.Worksheet.Finalize();
                document.Close();

                excelStream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"storage_inbox_{now:yyyyMMdd}.xlsx"
                };
            }
        }

        public static FileStreamResult ExportOutboxBlobs(GetProfileOutboxBlobsResponse data)
        {
            DateTime now = DateTime.Now;

            MemoryStream excelStream = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(excelStream, SpreadsheetDocumentType.Workbook, autoSave: true))
            {
                WorkbookPart workbookPart = document.AddNewPart<WorkbookPart>(
                    @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml",
                    "workbook");

                WorkbookStylesPart workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>("styles");
                workbookStylesPart.Stylesheet = new Stylesheet(
                    new NumberingFormats(
                        new NumberingFormat()
                        {
                            NumberFormatId = 165,
                            FormatCode = @"[$]dd\.mm\.yy;@",
                        }
                    ),
                    new Fonts(
                        new Font() // Default style
                    ),
                    new Fills(
                        new Fill() // Default fill
                    ),
                    new Borders(
                        new Border() // Default border
                    ),
                    new CellFormats(
                        new CellFormat() // Default cell format
                    ));

                workbookPart.Workbook = new Workbook(new Sheets());

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>("Sheet1");

                Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
                Sheet lastSheet = sheets.GetLastChild<Sheet>();

                sheets.AppendChild(
                    new Sheet()
                    {
                        Id = workbookPart.GetIdOfPart(worksheetPart),
                        SheetId = (lastSheet?.SheetId?.Value ?? 0) + 1,
                        Name = "Резултат",
                    });

                worksheetPart.InitNormalWorksheet();

                int titleFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 14.0);
                int headerFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 12.0);

                int titleTableCellFormat = workbookStylesPart.Stylesheet.AppendCellFormat(
                    verticalAlignment: VerticalAlignmentValues.Bottom,
                    horizontalAlignment: HorizontalAlignmentValues.Left,
                    wrapText: true,
                    fontId: titleFont);

                int headerTableCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                    verticalAlignment: VerticalAlignmentValues.Center,
                    horizontalAlignment: HorizontalAlignmentValues.Center,
                    wrapText: true,
                    fontId: headerFont);

                int dateCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                    // special date number format added in the Stylesheet
                    numberFormatId: 165);

                string[] headerRowColumnTitles = new string[]
                {
                    "Уникален №",
                    "Име на файл",
                    "Дата",
                    "Размер",
                    "Съобщение №",
                    "Съобщение",
                };

                Row titleRow = worksheetPart.Worksheet.AppendRelativeRow();

                titleRow.AppendRelativeInlineStringCell(
                    text: $"Експорт на първите {SystemConstants.ExportSize} резултата от списък Хранилище (Изпратени) към дата {now.ToString(SystemConstants.DateTimeFormat)}",
                    styleIndex: titleTableCellFormat);
                worksheetPart.Worksheet
                    .AppendMergeCell($"A{titleRow.RowIndex}:" +
                        $"{OpenXmlExtensions.ColumnIdToColumnIndex(headerRowColumnTitles.Length - 1)}{titleRow.RowIndex}");

                double primaryColWidth = 13;
                worksheetPart.Worksheet.GetColumns()
                    .AppendCustomWidthColumn(1, 1, primaryColWidth)
                    .AppendCustomWidthColumn(2, 2, primaryColWidth * 4)
                    .AppendCustomWidthColumn(3, 3, primaryColWidth)
                    .AppendCustomWidthColumn(4, 4, primaryColWidth)
                    .AppendCustomWidthColumn(5, 5, primaryColWidth)
                    .AppendCustomWidthColumn(6, 6, primaryColWidth * 4);

                Row headerRow = worksheetPart.Worksheet.AppendRelativeRow();
                foreach (string columnTitle in headerRowColumnTitles)
                {
                    headerRow
                        .AppendRelativeInlineStringCell(
                            text: $"{columnTitle}",
                            styleIndex: headerTableCell);
                }

                foreach (var blob in data.Result)
                {
                    Row blobRow = worksheetPart.Worksheet.AppendRelativeRow();

                    blobRow.AppendRelativeNumberCell(
                        number: blob.BlobId);

                    blobRow.AppendRelativeInlineStringCell(
                        text: blob.FileName);

                    blobRow.AppendRelativeDateCell(
                        date: blob.CreateDate.ToLocalDateTime(),
                        styleIndex: dateCell);

                    blobRow.AppendRelativeInlineStringCell(
                        text: Utils.Utils.FormatSize(Convert.ToUInt64(blob.Size)));

                    blobRow.AppendRelativeNumberCell(
                        number: blob.MessageId);

                    blobRow.AppendRelativeInlineStringCell(
                        text: blob.MessageSubject);
                }

                worksheetPart.Worksheet.Finalize();
                document.Close();

                excelStream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"storage_outbox_{now:yyyyMMdd}.xlsx"
                };
            }
        }

        public static FileStreamResult ExportTicketsInbox(ED.DomainServices.Tickets.InboxResponse data)
        {
            DateTime now = DateTime.Now;

            MemoryStream excelStream = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(excelStream, SpreadsheetDocumentType.Workbook, autoSave: true))
            {
                WorkbookPart workbookPart = document.AddNewPart<WorkbookPart>(
                    @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml",
                    "workbook");

                WorkbookStylesPart workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>("styles");
                workbookStylesPart.Stylesheet = new Stylesheet(
                    new NumberingFormats(
                        new NumberingFormat()
                        {
                            NumberFormatId = 165,
                            FormatCode = @"[$]dd\.mm\.yy;@",
                        }
                    ),
                    new Fonts(
                        new Font() // Default style
                    ),
                    new Fills(
                        new Fill() // Default fill
                    ),
                    new Borders(
                        new Border() // Default border
                    ),
                    new CellFormats(
                        new CellFormat() // Default cell format
                    ));

                workbookPart.Workbook = new Workbook(new Sheets());

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>("Sheet1");

                Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
                Sheet lastSheet = sheets.GetLastChild<Sheet>();

                sheets.AppendChild(
                    new Sheet()
                    {
                        Id = workbookPart.GetIdOfPart(worksheetPart),
                        SheetId = (lastSheet?.SheetId?.Value ?? 0) + 1,
                        Name = "Резултат",
                    });

                worksheetPart.InitNormalWorksheet();

                int titleFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 14.0);
                int headerFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 12.0);

                int titleTableCellFormat = workbookStylesPart.Stylesheet.AppendCellFormat(
                    verticalAlignment: VerticalAlignmentValues.Bottom,
                    horizontalAlignment: HorizontalAlignmentValues.Left,
                    wrapText: true,
                    fontId: titleFont);

                int headerTableCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                    verticalAlignment: VerticalAlignmentValues.Center,
                    horizontalAlignment: HorizontalAlignmentValues.Center,
                    wrapText: true,
                    fontId: headerFont);

                int dateCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                    // special date number format added in the Stylesheet
                    numberFormatId: 165);

                string[] headerRowColumnTitles =
                new string[]
                {
                    "Id",
                    "Профил подател",
                    "Дата на изпращане",
                    "Заглавие",
                    "Тип",
                    "Дата на нарушение",
                    "Статус",
                };

                Row titleRow = worksheetPart.Worksheet.AppendRelativeRow();

                titleRow.AppendRelativeInlineStringCell(
                    text: $"Експорт на първите {SystemConstants.ExportSize} резултата от списък Получени фишове към дата {now.ToString(SystemConstants.DateTimeFormat)}",
                    styleIndex: titleTableCellFormat);
                worksheetPart.Worksheet
                    .AppendMergeCell($"A{titleRow.RowIndex}:" +
                        $"{OpenXmlExtensions.ColumnIdToColumnIndex(headerRowColumnTitles.Length - 1)}{titleRow.RowIndex}");

                double primaryColWidth = 13;
                worksheetPart.Worksheet.GetColumns()
                    .AppendCustomWidthColumn(1, 1, primaryColWidth)
                    .AppendCustomWidthColumn(2, 2, primaryColWidth * 4)
                    .AppendCustomWidthColumn(3, 3, primaryColWidth)
                    .AppendCustomWidthColumn(4, 4, primaryColWidth * 4)
                    .AppendCustomWidthColumn(5, 5, primaryColWidth)
                    .AppendCustomWidthColumn(6, 6, primaryColWidth);

                Row headerRow = worksheetPart.Worksheet.AppendRelativeRow();
                foreach (string columnTitle in headerRowColumnTitles)
                {
                    headerRow
                        .AppendRelativeInlineStringCell(
                            text: $"{columnTitle}",
                            styleIndex: headerTableCell);
                }

                foreach (var ticket in data.Result)
                {
                    Row ticketRow = worksheetPart.Worksheet.AppendRelativeRow();

                    ticketRow.AppendRelativeNumberCell(
                        number: ticket.MessageId);

                    ticketRow.AppendRelativeInlineStringCell(
                        text: ticket.SenderProfileName);

                    ticketRow.AppendRelativeDateCell(
                        date: ticket.DateSent.ToLocalDateTime(),
                        styleIndex: dateCell);

                    ticketRow.AppendRelativeInlineStringCell(
                        text: ticket.Subject);

                    ticketRow.AppendRelativeInlineStringCell(
                        text: ticket.Type);

                    ticketRow.AppendRelativeDateCell(
                        date: ticket.ViolationDate.ToLocalDateTime(),
                        styleIndex: dateCell);

                    ticketRow.AppendRelativeInlineStringCell(
                        text: ticket.Status.ToString());
                }

                worksheetPart.Worksheet.Finalize();
                document.Close();

                excelStream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"ticket_inbox_{now:yyyyMMdd}.xlsx"
                };
            }
        }
    }
}
