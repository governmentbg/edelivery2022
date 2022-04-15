using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ED.AdminPanel.Utils;
using ED.DomainServices;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

#nullable enable

namespace ED.AdminPanel.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class ReportsController
    {
        public async Task<ActionResult<object>> ExportReceivedMessagesAsync(
            [FromQuery, Required][DateTimeModelBinder] DateTime? fromDate,
            [FromQuery, Required][DateTimeModelBinder] DateTime? toDate,
            [FromQuery, Required][DateTimeModelBinder] DateTime? requestDate,
            [FromQuery] int? recipientProfileId,
            [FromQuery] int? senderProfileId,
            [FromServices] Admin.AdminClient adminClient,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            fromDate = fromDate ?? throw new ArgumentNullException(nameof(fromDate));
            toDate = toDate ?? throw new ArgumentNullException(nameof(toDate));
            requestDate = requestDate ?? throw new ArgumentNullException(nameof(requestDate));

            int currentUserId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserId();

            GetReceivedMessageReportResponse report =
                await adminClient.GetReceivedMessageReportAsync(
                    new GetReceivedMessageReportRequest
                    {
                        AdminUserId = currentUserId,
                        FromDate = fromDate?.ToTimestamp(),
                        ToDate = toDate?.ToTimestamp(),
                        RecipientProfileId = recipientProfileId,
                        SenderProfileId = senderProfileId,
                        Offset = 0,
                        Limit = int.MaxValue,
                    },
                    cancellationToken: ct);

            // do not dispose the stream, this will be done by the FileStreamResult
            var excelStream = new MemoryStream();
            using var document = SpreadsheetDocument.Create(excelStream, SpreadsheetDocumentType.Workbook, autoSave: true);

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

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>("Sheet1");

            var sheets = workbookPart.Workbook.GetFirstChild<Sheets>()!;
            var lastSheet = sheets.GetLastChild<Sheet>();

            sheets.AppendChild(
                new Sheet()
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = (lastSheet?.SheetId?.Value ?? 0) + 1,
                    Name = "Резултат",
                });

            worksheetPart.InitNormalWorksheet();

            var titleFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 14.0);
            var headerFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 12.0);

            var titleTableCellFormat = workbookStylesPart.Stylesheet.AppendCellFormat(
                verticalAlignment: VerticalAlignmentValues.Bottom,
                horizontalAlignment: HorizontalAlignmentValues.Left,
                wrapText: true,
                fontId: titleFont);

            var headerTableCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                verticalAlignment: VerticalAlignmentValues.Center,
                horizontalAlignment: HorizontalAlignmentValues.Center,
                wrapText: true,
                fontId: headerFont);

            var dateCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                // special date number format added in the Stylesheet
                numberFormatId: 165);

            string[] headerRowColumnTitles =
                new string[]
                {
                    "Подател",
                    "Профил подател",
                    "Целева група подател",
                    "Получател",
                    "Профил получател",
                    "Целева група получател",
                    "Наименование",
                    "Дата на изпращане",
                    "Дата на получаване"
                };

            var titleRow = worksheetPart.Worksheet.AppendRelativeRow();

            titleRow
                .AppendRelativeInlineStringCell(
                    text: $"Справка получени съобщения от {fromDate?.ToString(Constants.DateTimeFormat)} до {toDate?.ToString(Constants.DateTimeFormat)} / Заявката е направена на {requestDate?.ToString(Constants.DateTimeFormat)}",
                    styleIndex: titleTableCellFormat);
            worksheetPart.Worksheet
                .AppendMergeCell($"A{titleRow.RowIndex}:" +
                    $"{OpenXmlExtensions.ColumnIdToColumnIndex(headerRowColumnTitles.Length - 1)}{titleRow.RowIndex}");

            double primaryColWidth = 13;
            worksheetPart.Worksheet.GetColumns()
                .AppendCustomWidthColumn(1, 1, primaryColWidth * 2)
                .AppendCustomWidthColumn(2, 2, primaryColWidth)
                .AppendCustomWidthColumn(3, 3, primaryColWidth * 3)
                .AppendCustomWidthColumn(4, 4, primaryColWidth)
                .AppendCustomWidthColumn(5, 5, primaryColWidth);

            var headerRow = worksheetPart.Worksheet.AppendRelativeRow();
            foreach (var columnTitle in headerRowColumnTitles)
            {
                headerRow
                    .AppendRelativeInlineStringCell(
                        text: $"{columnTitle}",
                        styleIndex: headerTableCell);
            }

            foreach (var message in report.Result)
            {
                var messageRow = worksheetPart.Worksheet.AppendRelativeRow();

                messageRow.AppendRelativeInlineStringCell(
                    text: message.SenderProfileName);

                messageRow.AppendRelativeInlineStringCell(
                    text: message.IsSenderProfileActivated ? "Активиран" : "Неактивиран");

                messageRow.AppendRelativeInlineStringCell(
                    text: message.SenderProfileTargetGroupName);

                messageRow.AppendRelativeInlineStringCell(
                    text: message.RecipientProfileName);

                messageRow.AppendRelativeInlineStringCell(
                    text: message.IsRecipientProfileActivated ? "Активиран" : "Неактивиран");

                messageRow.AppendRelativeInlineStringCell(
                    text: message.RecipientProfileTargetGroupName);

                messageRow.AppendRelativeInlineStringCell(
                    text: message.MessageSubject);

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
                FileDownloadName = "audit.xlsx"
            };
        }
        public async Task<ActionResult<object>> ExportSentMessagesAsync(
            [FromQuery, Required][DateTimeModelBinder] DateTime? fromDate,
            [FromQuery, Required][DateTimeModelBinder] DateTime? toDate,
            [FromQuery, Required][DateTimeModelBinder] DateTime? requestDate,
            [FromQuery] int? recipientProfileId,
            [FromQuery] int? senderProfileId,
            [FromServices] Admin.AdminClient adminClient,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            fromDate = fromDate ?? throw new ArgumentNullException(nameof(fromDate));
            toDate = toDate ?? throw new ArgumentNullException(nameof(toDate));
            requestDate = requestDate ?? throw new ArgumentNullException(nameof(requestDate));

            int currentUserId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserId();

            GetSentMessageReportResponse report =
                await adminClient.GetSentMessageReportAsync(
                    new GetSentMessageReportRequest
                    {
                        AdminUserId = currentUserId,
                        FromDate = fromDate?.ToTimestamp(),
                        ToDate = toDate?.ToTimestamp(),
                        RecipientProfileId = recipientProfileId,
                        SenderProfileId = senderProfileId,
                        Offset = 0,
                        Limit = int.MaxValue,
                    },
                    cancellationToken: ct);

            // do not dispose the stream, this will be done by the FileStreamResult
            var excelStream = new MemoryStream();
            using var document = SpreadsheetDocument.Create(excelStream, SpreadsheetDocumentType.Workbook, autoSave: true);

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

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>("Sheet1");

            var sheets = workbookPart.Workbook.GetFirstChild<Sheets>()!;
            var lastSheet = sheets.GetLastChild<Sheet>();

            sheets.AppendChild(
                new Sheet()
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = (lastSheet?.SheetId?.Value ?? 0) + 1,
                    Name = "Резултат",
                });

            worksheetPart.InitNormalWorksheet();

            var titleFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 14.0);
            var headerFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 12.0);

            var titleTableCellFormat = workbookStylesPart.Stylesheet.AppendCellFormat(
                verticalAlignment: VerticalAlignmentValues.Bottom,
                horizontalAlignment: HorizontalAlignmentValues.Left,
                wrapText: true,
                fontId: titleFont);

            var headerTableCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                verticalAlignment: VerticalAlignmentValues.Center,
                horizontalAlignment: HorizontalAlignmentValues.Center,
                wrapText: true,
                fontId: headerFont);

            var dateCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                // special date number format added in the Stylesheet
                numberFormatId: 165);

            string[] headerRowColumnTitles =
                new string[]
                {
                    "Подател",
                    "Профил подател",
                    "Целева група подател",
                    "Получател",
                    "Профил получател",
                    "Целева група получател",
                    "Наименование",
                    "Дата на изпращане",
                    "Дата на получаване"
                };

            var titleRow = worksheetPart.Worksheet.AppendRelativeRow();

            titleRow
                .AppendRelativeInlineStringCell(
                    text: $"Справка изпратени съобщения от {fromDate?.ToString(Constants.DateTimeFormat)} до {toDate?.ToString(Constants.DateTimeFormat)} / Заявката е направена на {requestDate?.ToString(Constants.DateTimeFormat)}",
                    styleIndex: titleTableCellFormat);
            worksheetPart.Worksheet
                .AppendMergeCell($"A{titleRow.RowIndex}:" +
                    $"{OpenXmlExtensions.ColumnIdToColumnIndex(headerRowColumnTitles.Length - 1)}{titleRow.RowIndex}");

            double primaryColWidth = 13;
            worksheetPart.Worksheet.GetColumns()
                .AppendCustomWidthColumn(1, 1, primaryColWidth * 2)
                .AppendCustomWidthColumn(2, 2, primaryColWidth)
                .AppendCustomWidthColumn(3, 3, primaryColWidth * 3)
                .AppendCustomWidthColumn(4, 4, primaryColWidth)
                .AppendCustomWidthColumn(5, 5, primaryColWidth);

            var headerRow = worksheetPart.Worksheet.AppendRelativeRow();
            foreach (var columnTitle in headerRowColumnTitles)
            {
                headerRow
                    .AppendRelativeInlineStringCell(
                        text: $"{columnTitle}",
                        styleIndex: headerTableCell);
            }

            foreach (var message in report.Result)
            {
                var messageRow = worksheetPart.Worksheet.AppendRelativeRow();

                messageRow.AppendRelativeInlineStringCell(
                    text: message.SenderProfileName);

                messageRow.AppendRelativeInlineStringCell(
                    text: message.IsSenderProfileActivated ? "Активиран" : "Неактивиран");

                messageRow.AppendRelativeInlineStringCell(
                    text: message.SenderProfileTargetGroupName);

                messageRow.AppendRelativeInlineStringCell(
                    text: message.RecipientProfileName);

                messageRow.AppendRelativeInlineStringCell(
                    text: message.IsRecipientProfileActivated ? "Активиран" : "Неактивиран");

                messageRow.AppendRelativeInlineStringCell(
                    text: message.RecipientProfileTargetGroupName);

                messageRow.AppendRelativeInlineStringCell(
                    text: message.MessageSubject);

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
                FileDownloadName = "audit.xlsx"
            };
        }

        public async Task<ActionResult<object>> ExportDelayedMessagesAsync(
           [FromQuery, Required] int? delay,
           [FromQuery, Required] int? targetGroupId,
           [FromQuery] string? recipientProfileId,
           [FromQuery][DateTimeModelBinder] DateTime fromDate,
           [FromServices] Admin.AdminClient adminClient,
           [FromServices] IHttpContextAccessor httpContextAccessor,
           CancellationToken ct)
        {
            delay = delay ?? throw new ArgumentNullException(nameof(delay));
            targetGroupId = targetGroupId ?? throw new ArgumentNullException(nameof(targetGroupId));
            int currentUserId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserId();

            GetDelayedMessagesReportResponse report =
                await adminClient.GetDelayedMessagesReportAsync(
                    new GetDelayedMessagesReportRequest()
                    {
                        AdminUserId = currentUserId,
                        Delay = delay.Value,
                        TargetGroupId = targetGroupId.Value,
                        ProfileId = string.IsNullOrEmpty(recipientProfileId)
                            ? null
                            : int.Parse(recipientProfileId),
                        Offset = 0,
                        Limit = int.MaxValue
                    },
                    cancellationToken: ct);

            // do not dispose the stream, this will be done by the FileStreamResult
            var excelStream = new MemoryStream();
            using var document = SpreadsheetDocument.Create(excelStream, SpreadsheetDocumentType.Workbook, autoSave: true);

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

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>("Sheet1");

            var sheets = workbookPart.Workbook.GetFirstChild<Sheets>()!;
            var lastSheet = sheets.GetLastChild<Sheet>();

            sheets.AppendChild(
                new Sheet()
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = (lastSheet?.SheetId?.Value ?? 0) + 1,
                    Name = "Резултат",
                });

            worksheetPart.InitNormalWorksheet();

            var titleFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 14.0);
            var headerFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 12.0);

            var titleTableCellFormat = workbookStylesPart.Stylesheet.AppendCellFormat(
                verticalAlignment: VerticalAlignmentValues.Bottom,
                horizontalAlignment: HorizontalAlignmentValues.Left,
                wrapText: true,
                fontId: titleFont);

            var headerTableCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                verticalAlignment: VerticalAlignmentValues.Center,
                horizontalAlignment: HorizontalAlignmentValues.Center,
                wrapText: true,
                fontId: headerFont);

            var dateCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                // special date number format added in the Stylesheet
                numberFormatId: 165);

            string[] headerRowColumnTitles =
                new string[]
                {
                    "Получател",
                    "Профил получател",
                    "Целева група получател",
                    "Email получател",
                    "Връчител",
                    "Email връчител",
                    "Съобщение",
                    "Дата на изпращане",
                    "Дни на забавяне на връчването"
                };

            var titleRow = worksheetPart.Worksheet.AppendRelativeRow();

            titleRow
                .AppendRelativeInlineStringCell(
                    text: $"Справка забавени съобщения към {fromDate:dd-MM-yyyy}",
                    styleIndex: titleTableCellFormat);
            worksheetPart.Worksheet
                .AppendMergeCell($"A{titleRow.RowIndex}:" +
                    $"{OpenXmlExtensions.ColumnIdToColumnIndex(headerRowColumnTitles.Length - 1)}{titleRow.RowIndex}");

            double primaryColWidth = 13;
            worksheetPart.Worksheet.GetColumns()
                .AppendCustomWidthColumn(1, 1, primaryColWidth * 2)
                .AppendCustomWidthColumn(2, 2, primaryColWidth)
                .AppendCustomWidthColumn(3, 3, primaryColWidth * 3)
                .AppendCustomWidthColumn(4, 4, primaryColWidth)
                .AppendCustomWidthColumn(5, 5, primaryColWidth);

            var headerRow = worksheetPart.Worksheet.AppendRelativeRow();
            foreach (var columnTitle in headerRowColumnTitles)
            {
                headerRow
                    .AppendRelativeInlineStringCell(
                        text: $"{columnTitle}",
                        styleIndex: headerTableCell);
            }

            foreach (var message in report.Result)
            {
                var messageRow = worksheetPart.Worksheet.AppendRelativeRow();

                messageRow.AppendRelativeInlineStringCell(
                    text: message.RecipientProfileName);

                messageRow.AppendRelativeInlineStringCell(
                    text: message.IsRecipientProfileActivated ? "Активиран" : "Неактивиран");

                messageRow.AppendRelativeInlineStringCell(
                    text: message.RecipientProfileTargetGroupName);

                messageRow.AppendRelativeInlineStringCell(
                    text: message.RecipientEmail);

                messageRow.AppendRelativeInlineStringCell(
                    text: message.SenderProfileName);

                messageRow.AppendRelativeInlineStringCell(
                    text: message.SenderEmail);

                messageRow.AppendRelativeInlineStringCell(
                    text: message.MessageSubject);

                messageRow.AppendRelativeDateCell(
                    date: message.DateSent.ToLocalDateTime(),
                    styleIndex: dateCell);

                messageRow.AppendRelativeNumberCell(
                    number: message.Delay);
            }

            worksheetPart.Worksheet.Finalize();
            document.Close();

            excelStream.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "audit.xlsx"
            };
        }

        public async Task<ActionResult<object>> ExportEFormsAsync(
            [FromQuery, Required][DateTimeModelBinder] DateTime? fromDate,
            [FromQuery, Required][DateTimeModelBinder] DateTime? toDate,
            [FromQuery] string? eFormServiceNumber,
            [FromServices] Admin.AdminClient adminClient,
            [FromServices] IHttpContextAccessor httpContextAccessor,
           CancellationToken ct)
        {

            fromDate = fromDate ?? throw new ArgumentNullException(nameof(fromDate));
            toDate = toDate ?? throw new ArgumentNullException(nameof(toDate));

            int currentUserId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserId();

            GetEFormReportResponse report =
                await adminClient.GetEFormReportAsync(
                    new GetEFormReportRequest()
                    {
                        AdminUserId = currentUserId,
                        FromDate = fromDate?.ToTimestamp(),
                        ToDate = toDate?.ToTimestamp(),
                        EFormServiceNumber = eFormServiceNumber ?? string.Empty,
                        Offset = 0,
                        Limit = int.MaxValue
                    },
                    cancellationToken: ct);

            // do not dispose the stream, this will be done by the FileStreamResult
            var excelStream = new MemoryStream();
            using var document = SpreadsheetDocument.Create(excelStream, SpreadsheetDocumentType.Workbook, autoSave: true);

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

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>("Sheet1");

            var sheets = workbookPart.Workbook.GetFirstChild<Sheets>()!;
            var lastSheet = sheets.GetLastChild<Sheet>();

            sheets.AppendChild(
                new Sheet()
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = (lastSheet?.SheetId?.Value ?? 0) + 1,
                    Name = "Резултат",
                });

            worksheetPart.InitNormalWorksheet();

            var titleFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 14.0);
            var headerFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 12.0);

            var titleTableCellFormat = workbookStylesPart.Stylesheet.AppendCellFormat(
                verticalAlignment: VerticalAlignmentValues.Bottom,
                horizontalAlignment: HorizontalAlignmentValues.Left,
                wrapText: true,
                fontId: titleFont);

            var headerTableCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                verticalAlignment: VerticalAlignmentValues.Center,
                horizontalAlignment: HorizontalAlignmentValues.Center,
                wrapText: true,
                fontId: headerFont);

            var dateCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                // special date number format added in the Stylesheet
                numberFormatId: 165);

            string[] headerRowColumnTitles =
                new string[]
                {
                    "Наименование",
                    "Брой"
                };

            var titleRow = worksheetPart.Worksheet.AppendRelativeRow();

            titleRow
                .AppendRelativeInlineStringCell(
                    text: $"Справка Електронни Форми от {fromDate?.ToString(Constants.DateTimeFormat)} до {toDate?.ToString(Constants.DateTimeFormat)}",
                    styleIndex: titleTableCellFormat);
            worksheetPart.Worksheet
                .AppendMergeCell($"A{titleRow.RowIndex}:" +
                    $"{OpenXmlExtensions.ColumnIdToColumnIndex(headerRowColumnTitles.Length + 3)}{titleRow.RowIndex}");//+3 because not enough space for the text 

            double primaryColWidth = 13;
            worksheetPart.Worksheet.GetColumns()
                .AppendCustomWidthColumn(1, 1, primaryColWidth * 2)
                .AppendCustomWidthColumn(2, 2, primaryColWidth)
                .AppendCustomWidthColumn(3, 3, primaryColWidth * 3)
                .AppendCustomWidthColumn(4, 4, primaryColWidth)
                .AppendCustomWidthColumn(5, 5, primaryColWidth);

            var headerRow = worksheetPart.Worksheet.AppendRelativeRow();
            foreach (var columnTitle in headerRowColumnTitles)
            {
                headerRow
                    .AppendRelativeInlineStringCell(
                        text: $"{columnTitle}",
                        styleIndex: headerTableCell);
            }

            foreach (var message in report.Result)
            {
                var messageRow = worksheetPart.Worksheet.AppendRelativeRow();

                messageRow.AppendRelativeInlineStringCell(
                    text: message.MessageSubject);
                messageRow.AppendRelativeNumberCell(
                    number: message.Count);
            }

            worksheetPart.Worksheet.Finalize();
            document.Close();

            excelStream.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "audit.xlsx"
            };
        }

        public async Task<ActionResult<object>> ExportStatisticsAsync(
            [FromQuery, Required][DateTimeModelBinder] DateTime? toDate,
            [FromServices] Admin.AdminClient adminClient,
            [FromServices] IHttpContextAccessor httpContextAccessor,
           CancellationToken ct)
        {
            toDate = toDate ?? throw new ArgumentNullException(nameof(toDate));

            int currentUserId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserId();

            GetStatisticsReportResponse report =
                await adminClient.GetStatisticsReportAsync(
                    new GetStatisticsReportRequest()
                    {
                        AdminUserId = currentUserId
                    },
                    cancellationToken: ct);

            // do not dispose the stream, this will be done by the FileStreamResult
            var excelStream = new MemoryStream();
            using var document = SpreadsheetDocument.Create(excelStream, SpreadsheetDocumentType.Workbook, autoSave: true);

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

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>("Sheet1");

            var sheets = workbookPart.Workbook.GetFirstChild<Sheets>()!;
            var lastSheet = sheets.GetLastChild<Sheet>();

            sheets.AppendChild(
                new Sheet()
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = (lastSheet?.SheetId?.Value ?? 0) + 1,
                    Name = "Резултат",
                });

            worksheetPart.InitNormalWorksheet();

            var titleFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 14.0);
            var headerFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 12.0);

            var titleTableCellFormat = workbookStylesPart.Stylesheet.AppendCellFormat(
                verticalAlignment: VerticalAlignmentValues.Bottom,
                horizontalAlignment: HorizontalAlignmentValues.Left,
                wrapText: true,
                fontId: titleFont);

            var headerTableCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                verticalAlignment: VerticalAlignmentValues.Center,
                horizontalAlignment: HorizontalAlignmentValues.Center,
                wrapText: true,
                fontId: headerFont);

            var dateCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                // special date number format added in the Stylesheet
                numberFormatId: 165);

            string[] headerRowColumnTitles =
                new string[]
                {
                    "Обща статистика",
                    ""
                };

            var titleRow = worksheetPart.Worksheet.AppendRelativeRow();

            titleRow
                .AppendRelativeInlineStringCell(
                    text: $"Статистика към {toDate.Value.ToString(Constants.DateTimeFormat)}",
                    styleIndex: titleTableCellFormat);
            worksheetPart.Worksheet
                .AppendMergeCell($"A{titleRow.RowIndex}:" +
                    $"{OpenXmlExtensions.ColumnIdToColumnIndex(headerRowColumnTitles.Length - 1)}{titleRow.RowIndex}");

            double primaryColWidth = 13;
            worksheetPart.Worksheet.GetColumns()
                .AppendCustomWidthColumn(1, 1, primaryColWidth * 2)
                .AppendCustomWidthColumn(2, 2, primaryColWidth)
                .AppendCustomWidthColumn(3, 3, primaryColWidth * 3)
                .AppendCustomWidthColumn(4, 4, primaryColWidth)
                .AppendCustomWidthColumn(5, 5, primaryColWidth);

            var headerRow = worksheetPart.Worksheet.AppendRelativeRow();
            foreach (var columnTitle in headerRowColumnTitles)
            {
                headerRow
                    .AppendRelativeInlineStringCell(
                        text: $"{columnTitle}",
                        styleIndex: headerTableCell);
            }

            CreateMessageRow(worksheetPart, "Брой регистрирани потребители", report.TotalUsers);
            foreach (var message in report.TargetGroupsCount)
            {
                CreateMessageRow(worksheetPart, $"Целева група - {message.Key} ", message.Value);
            }
            CreateMessageRow(worksheetPart, "Брой изпратени съобщения(общо)", report.TotalMessages);
            CreateMessageRow(worksheetPart, "Брой изпратени съобщения за последните 30 дни", report.TotalMessagesLast30Days);
            CreateMessageRow(worksheetPart, "Брой изпратени съобщения за последните 10 дни", report.TotalMessagesLast10Days);
            CreateMessageRow(worksheetPart, "Брой изпратени съобщения за днес(за деня)", report.TotalMessagesToday);

            worksheetPart.Worksheet.Finalize();
            document.Close();

            excelStream.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "audit.xlsx"
            };

            static Row CreateMessageRow(WorksheetPart worksheetPart, string text, int? number = null)
            {
                var messageRow = worksheetPart.Worksheet.AppendRelativeRow();
                messageRow.AppendRelativeInlineStringCell(text: text);
                if (number != null)
                {
                    messageRow.AppendRelativeNumberCell(number: number);
                }
                return messageRow;
            }
        }

        public async Task<ActionResult<object>> ExportNotificationsAsync(
            [FromQuery, Required][DateTimeModelBinder] DateTime fromDate,
            [FromQuery, Required][DateTimeModelBinder] DateTime toDate,
            [FromServices] Admin.AdminClient adminClient,
            [FromServices] IHttpContextAccessor httpContextAccessor,
           CancellationToken ct)
        {
            int currentUserId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserId();

            GetNotificationsReportResponse report =
                await adminClient.GetNotificationsReportAsync(
                    new GetNotificationsReportRequest()
                    {
                        AdminUserId = currentUserId,
                        FromDate = fromDate.ToTimestamp(),
                        ToDate = toDate.ToTimestamp(),
                    },
                    cancellationToken: ct);

            // do not dispose the stream, this will be done by the FileStreamResult
            var excelStream = new MemoryStream();
            using var document = SpreadsheetDocument.Create(excelStream, SpreadsheetDocumentType.Workbook, autoSave: true);

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

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>("Sheet1");

            var sheets = workbookPart.Workbook.GetFirstChild<Sheets>()!;
            var lastSheet = sheets.GetLastChild<Sheet>();

            sheets.AppendChild(
                new Sheet()
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = (lastSheet?.SheetId?.Value ?? 0) + 1,
                    Name = "Резултат",
                });

            worksheetPart.InitNormalWorksheet();

            var titleFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 14.0);
            var headerFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 12.0);

            var titleTableCellFormat = workbookStylesPart.Stylesheet.AppendCellFormat(
                verticalAlignment: VerticalAlignmentValues.Bottom,
                horizontalAlignment: HorizontalAlignmentValues.Left,
                wrapText: true,
                fontId: titleFont);

            var headerTableCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                verticalAlignment: VerticalAlignmentValues.Center,
                horizontalAlignment: HorizontalAlignmentValues.Center,
                wrapText: true,
                fontId: headerFont);

            var dateCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                // special date number format added in the Stylesheet
                numberFormatId: 165);

            string[] headerRowColumnTitles =
                new string[]
                {
                    "Тип",
                    "Изпратени",
                    "Грешни",
                    "Тотал"
                };

            string[] headerRowColumnTitlesDatesTable =
                new string[]
                {
                    "Ден",
                    "Тип",
                    "Изпратени",
                    "Грешни",
                    "Тотал"
                };

            var titleRow = worksheetPart.Worksheet.AppendRelativeRow();

            titleRow
                .AppendRelativeInlineStringCell(
                    text: $"Справка получени нотификации от {fromDate:dd-MM-yyyy} до {toDate:dd-MM-yyyy}",
                    styleIndex: titleTableCellFormat);
            worksheetPart.Worksheet
                .AppendMergeCell($"A{titleRow.RowIndex}:" +
                    $"{OpenXmlExtensions.ColumnIdToColumnIndex(headerRowColumnTitles.Length - 1)}{titleRow.RowIndex}");

            double primaryColWidth = 13;
            worksheetPart.Worksheet.GetColumns()
                .AppendCustomWidthColumn(1, 1, primaryColWidth * 2)
                .AppendCustomWidthColumn(2, 2, primaryColWidth)
                .AppendCustomWidthColumn(3, 3, primaryColWidth * 3)
                .AppendCustomWidthColumn(4, 4, primaryColWidth)
                .AppendCustomWidthColumn(5, 5, primaryColWidth);

            var headerRow = worksheetPart.Worksheet.AppendRelativeRow();

            foreach (var columnTitle in headerRowColumnTitles)
            {
                headerRow
                    .AppendRelativeInlineStringCell(
                        text: $"{columnTitle}",
                        styleIndex: headerTableCell);
            }

            foreach (var message in report.Result
                .GroupBy(q => q.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    Sent = g.Sum(sent => sent.Sent),
                    Error = g.Sum(error => error.Error),
                    SumSentError = g.Sum(all => all.Error + all.Sent)
                }))
            {
                var messageRow = worksheetPart.Worksheet.AppendRelativeRow();

                messageRow.AppendRelativeInlineStringCell(
                    text: message.Type.ToString());

                messageRow.AppendRelativeNumberCell(
                    number: message.Sent);

                messageRow.AppendRelativeNumberCell(
                    number: message.Error);

                messageRow.AppendRelativeNumberCell(
                    number: message.SumSentError);
            }

            //Second Table with information for each date
            var emptyRow = worksheetPart.Worksheet.AppendRelativeRow(); //Empty row to separate tables
            var headerRow2 = worksheetPart.Worksheet.AppendRelativeRow();
            foreach (var columnTitle in headerRowColumnTitlesDatesTable)
            {
                headerRow2
                    .AppendRelativeInlineStringCell(
                        text: $"{columnTitle}",
                        styleIndex: headerTableCell);
            }

            if (report.Result.Any())
            {
                foreach (DateTime day in this.EachDay(fromDate, toDate))
                {
                    GetNotificationsReportResponse.Types.NotificationsMessage? sms = report.Result
                        .FirstOrDefault(x => x.Date.ToLocalDateTime() == day && x.Type == NotificationType.Sms);

                    var smsMessageRow = worksheetPart.Worksheet.AppendRelativeRow();
                    smsMessageRow.AppendRelativeInlineStringCell(
                        text: day.Date.ToString("dd/MM/yyyy"));
                    smsMessageRow.AppendRelativeInlineStringCell(
                        text: (sms?.Type.ToString() ?? NotificationType.Sms.ToString()));
                    smsMessageRow.AppendRelativeNumberCell(
                        number: (sms?.Sent ?? 0));
                    smsMessageRow.AppendRelativeNumberCell(
                        number: (sms?.Error ?? 0));
                    smsMessageRow.AppendRelativeNumberCell(
                        number: ((sms?.Error ?? 0) + (sms?.Sent ?? 0)));

                    GetNotificationsReportResponse.Types.NotificationsMessage? email = report.Result
                        .FirstOrDefault(x => x.Date.ToLocalDateTime() == day && x.Type == NotificationType.Email);

                    var emailMessageRow = worksheetPart.Worksheet.AppendRelativeRow();
                    emailMessageRow.AppendRelativeInlineStringCell(
                        text: day.Date.ToString("dd/MM/yyyy"));
                    emailMessageRow.AppendRelativeInlineStringCell(
                        text: (email?.Type.ToString() ?? NotificationType.Email.ToString()));
                    emailMessageRow.AppendRelativeNumberCell(
                        number: (email?.Sent ?? 0));
                    emailMessageRow.AppendRelativeNumberCell(
                        number: (email?.Error ?? 0));
                    emailMessageRow.AppendRelativeNumberCell(
                        number: ((email?.Error ?? 0) + (email?.Sent ?? 0)));

                    GetNotificationsReportResponse.Types.NotificationsMessage? viber = report.Result
                        .FirstOrDefault(x => x.Date.ToLocalDateTime() == day && x.Type == NotificationType.Viber);

                    var viberMessageRow = worksheetPart.Worksheet.AppendRelativeRow();
                    viberMessageRow.AppendRelativeInlineStringCell(
                        text: day.Date.ToString("dd/MM/yyyy"));
                    viberMessageRow.AppendRelativeInlineStringCell(
                        text: (viber?.Type.ToString() ?? NotificationType.Viber.ToString()));
                    viberMessageRow.AppendRelativeNumberCell(
                        number: (viber?.Sent ?? 0));
                    viberMessageRow.AppendRelativeNumberCell(
                        number: (viber?.Error ?? 0));
                    viberMessageRow.AppendRelativeNumberCell(
                        number: ((viber?.Error ?? 0) + (viber?.Sent ?? 0)));
                }
            }

            worksheetPart.Worksheet.Finalize();
            document.Close();

            excelStream.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "audit.xlsx"
            };
        }

        public async Task<ActionResult<object>> ExportTimestampsAsync(
            [FromQuery, Required][DateTimeModelBinder] DateTime fromDate,
            [FromQuery, Required][DateTimeModelBinder] DateTime toDate,
            [FromQuery, Required][DateTimeModelBinder] DateTime? requestDate,
            [FromServices] Admin.AdminClient adminClient,
            [FromServices] IHttpContextAccessor httpContextAccessor,
           CancellationToken ct)
        {
            requestDate ??= DateTime.Now;

            int currentUserId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserId();

            GetTimestampsReportResponse report =
                await adminClient.GetTimestampsReportAsync(
                    new GetTimestampsReportRequest
                    {
                        AdminUserId = currentUserId,
                        FromDate = fromDate.ToTimestamp(),
                        ToDate = toDate.ToTimestamp(),
                    },
                    cancellationToken: ct);

            // do not dispose the stream, this will be done by the FileStreamResult
            var excelStream = new MemoryStream();
            using var document = SpreadsheetDocument.Create(excelStream, SpreadsheetDocumentType.Workbook, autoSave: true);

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

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>("Sheet1");

            var sheets = workbookPart.Workbook.GetFirstChild<Sheets>()!;
            var lastSheet = sheets.GetLastChild<Sheet>();

            sheets.AppendChild(
                new Sheet()
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = (lastSheet?.SheetId?.Value ?? 0) + 1,
                    Name = "Резултат",
                });

            worksheetPart.InitNormalWorksheet();

            var titleFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 14.0);
            var headerFont = workbookStylesPart.Stylesheet.AppendFont(bold: true, size: 12.0);

            var titleTableCellFormat = workbookStylesPart.Stylesheet.AppendCellFormat(
                verticalAlignment: VerticalAlignmentValues.Bottom,
                horizontalAlignment: HorizontalAlignmentValues.Left,
                wrapText: true,
                fontId: titleFont);

            var headerTableCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                verticalAlignment: VerticalAlignmentValues.Center,
                horizontalAlignment: HorizontalAlignmentValues.Center,
                wrapText: true,
                fontId: headerFont);

            var dateCell = workbookStylesPart.Stylesheet.AppendCellFormat(
                // special date number format added in the Stylesheet
                numberFormatId: 165);

            string[] headerRowColumnTitles =
                new string[]
                {
                    "",
                    "Брой"
                };

            var titleRow = worksheetPart.Worksheet.AppendRelativeRow();

            titleRow
                .AppendRelativeInlineStringCell(
                    text: $"Справка Timestamp заявки от {fromDate.ToString(Constants.DateTimeFormat)} до {toDate.ToString(Constants.DateTimeFormat)} / Заявката е направена на {requestDate?.ToString(Constants.DateTimeFormat)}",
                    styleIndex: titleTableCellFormat);
            worksheetPart.Worksheet
                .AppendMergeCell($"A{titleRow.RowIndex}:" +
                    $"{OpenXmlExtensions.ColumnIdToColumnIndex(headerRowColumnTitles.Length - 1)}{titleRow.RowIndex}");

            double primaryColWidth = 13;
            worksheetPart.Worksheet.GetColumns()
                .AppendCustomWidthColumn(1, 1, primaryColWidth * 4)
                .AppendCustomWidthColumn(2, 2, primaryColWidth);

            var headerRow = worksheetPart.Worksheet.AppendRelativeRow();
            foreach (var columnTitle in headerRowColumnTitles)
            {
                headerRow
                    .AppendRelativeInlineStringCell(
                        text: $"{columnTitle}",
                        styleIndex: headerTableCell);
            }

            var messageRow = worksheetPart.Worksheet.AppendRelativeRow();
            messageRow.AppendRelativeInlineStringCell(text: "Успешни");
            messageRow.AppendRelativeNumberCell(number: report.CountSuccess);

            messageRow = worksheetPart.Worksheet.AppendRelativeRow();
            messageRow.AppendRelativeInlineStringCell(text: "Неуспешни");
            messageRow.AppendRelativeNumberCell(number: report.CountError);

            messageRow = worksheetPart.Worksheet.AppendRelativeRow();
            messageRow.AppendRelativeInlineStringCell(text: "Общо");
            messageRow.AppendRelativeNumberCell(number: report.CountSuccess + report.CountError);

            worksheetPart.Worksheet.Finalize();
            document.Close();

            excelStream.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "audit.xlsx"
            };
        }

        protected IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
        {
            for (var day = from.Date; day.Date <= to.Date; day = day.AddDays(1))
            {
                yield return day;
            }
        }
    }
}
