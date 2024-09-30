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
using Grpc.Net.ClientFactory;
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
        private const int MaxRows = 1000;
        private readonly Admin.AdminClient adminClient;

        public ReportsController(GrpcClientFactory grpcClientFactory)
        {
            this.adminClient =
                grpcClientFactory.CreateClient<Admin.AdminClient>(Startup.GrpcReportsClient);
        }

        public async Task<ActionResult<object>> ExportReceivedMessagesAsync(
            [FromQuery, Required][DateTimeModelBinder] DateTime? fromDate,
            [FromQuery, Required][DateTimeModelBinder] DateTime? toDate,
            [FromQuery, Required][DateTimeModelBinder] DateTime? requestDate,
            [FromQuery] int? recipientProfileId,
            [FromQuery] int? senderProfileId,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            fromDate = fromDate ?? throw new ArgumentNullException(nameof(fromDate));
            toDate = toDate ?? throw new ArgumentNullException(nameof(toDate));
            requestDate = requestDate ?? throw new ArgumentNullException(nameof(requestDate));

            int currentUserId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserId();

            GetReceivedMessageReportResponse report =
                await this.adminClient.GetReceivedMessageReportAsync(
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
                FileDownloadName = "received_messages.xlsx"
            };
        }

        public async Task<ActionResult<object>> ExportSentMessagesAsync(
            [FromQuery, Required][DateTimeModelBinder] DateTime? fromDate,
            [FromQuery, Required][DateTimeModelBinder] DateTime? toDate,
            [FromQuery, Required][DateTimeModelBinder] DateTime? requestDate,
            [FromQuery] int? recipientProfileId,
            [FromQuery] int? senderProfileId,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            fromDate = fromDate ?? throw new ArgumentNullException(nameof(fromDate));
            toDate = toDate ?? throw new ArgumentNullException(nameof(toDate));
            requestDate = requestDate ?? throw new ArgumentNullException(nameof(requestDate));

            int currentUserId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserId();

            GetSentMessageReportResponse report =
                await this.adminClient.GetSentMessageReportAsync(
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
                    "Идентификатор",
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
                .AppendCustomWidthColumn(1, 1, primaryColWidth)
                .AppendCustomWidthColumn(2, 2, primaryColWidth * 2)
                .AppendCustomWidthColumn(3, 3, primaryColWidth)
                .AppendCustomWidthColumn(4, 4, primaryColWidth * 3)
                .AppendCustomWidthColumn(5, 5, primaryColWidth)
                .AppendCustomWidthColumn(6, 6, primaryColWidth);

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

                messageRow.AppendRelativeNumberCell(
                    number: message.MessageId);

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
                FileDownloadName = "sent_messages.xlsx"
            };
        }

        public async Task<ActionResult<object>> ExportDelayedMessagesAsync(
            [FromQuery, Required] int? delay,
            [FromQuery, Required] int? targetGroupId,
            [FromQuery] string? recipientProfileId,
            [FromQuery][DateTimeModelBinder] DateTime fromDate,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            delay = delay ?? throw new ArgumentNullException(nameof(delay));
            targetGroupId = targetGroupId ?? throw new ArgumentNullException(nameof(targetGroupId));
            int currentUserId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserId();

            GetDelayedMessagesReportResponse report =
                await this.adminClient.GetDelayedMessagesReportAsync(
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
                FileDownloadName = "delayed_messages.xlsx"
            };
        }

        public async Task<ActionResult<object>> ExportEFormsAsync(
            [FromQuery, Required][DateTimeModelBinder] DateTime? fromDate,
            [FromQuery, Required][DateTimeModelBinder] DateTime? toDate,
            [FromQuery] string? subject,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            fromDate = fromDate ?? throw new ArgumentNullException(nameof(fromDate));
            toDate = toDate ?? throw new ArgumentNullException(nameof(toDate));

            int currentUserId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserId();

            GetEFormReportResponse report =
                await this.adminClient.GetEFormReportAsync(
                    new GetEFormReportRequest()
                    {
                        AdminUserId = currentUserId,
                        FromDate = fromDate?.ToTimestamp(),
                        ToDate = toDate?.ToTimestamp(),
                        Subject = subject
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
                    "Получател",
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
                .AppendCustomWidthColumn(1, 1, primaryColWidth * 8)
                .AppendCustomWidthColumn(2, 2, primaryColWidth * 3)
                .AppendCustomWidthColumn(3, 3, primaryColWidth);

            var headerRow = worksheetPart.Worksheet.AppendRelativeRow();
            foreach (var columnTitle in headerRowColumnTitles)
            {
                headerRow
                    .AppendRelativeInlineStringCell(
                        text: $"{columnTitle}",
                        styleIndex: headerTableCell);
            }

            foreach (var groupbyMessageSubject in report.Result.GroupBy(e => e.MessageSubject).OrderBy(e => e.Key))
            {
                string serviceName = groupbyMessageSubject.Key;

                foreach (var record in groupbyMessageSubject.OrderBy(e => e.Recipient))
                {
                    var row = worksheetPart.Worksheet.AppendRelativeRow();

                    row.AppendRelativeInlineStringCell(
                        text: serviceName);
                    row.AppendRelativeInlineStringCell(
                        text: record.Recipient);
                    row.AppendRelativeNumberCell(
                        number: record.Count);
                }
            }

            worksheetPart.Worksheet.Finalize();
            document.Close();

            excelStream.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "eform_report.xlsx"
            };
        }

        public async Task<ActionResult<object>> ExportStatisticsAsync(
            [FromQuery, Required][DateTimeModelBinder] DateTime? toDate,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            toDate = toDate ?? throw new ArgumentNullException(nameof(toDate));

            int currentUserId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserId();

            GetStatisticsReportResponse report =
                await this.adminClient.GetStatisticsReportAsync(
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
                FileDownloadName = "statistics_report.xlsx"
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
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            int currentUserId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserId();

            GetNotificationsReportResponse report =
                await this.adminClient.GetNotificationsReportAsync(
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
                    "Тотал",
                };

            string[] headerRowColumnTitlesDatesTable =
                new string[]
                {
                    "Ден",
                    "Тип",
                    "Изпратени",
                    "Грешни",
                    "Тотал",
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
                .AppendCustomWidthColumn(1, 1, primaryColWidth)
                .AppendCustomWidthColumn(2, 2, primaryColWidth * 2)
                .AppendCustomWidthColumn(3, 3, primaryColWidth)
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

            {
                var groupedRecords = report.Result
                    .GroupBy(q => q.Type)
                    .Select(g => new
                    {
                        Type = g.Key,
                        Sent = g.Sum(sent => sent.Sent),
                        Error = g.Sum(error => error.Error),
                        Total = g.Sum(all => all.Error + all.Sent)
                    })
                    .ToList();

                var emails = groupedRecords
                    .Where(e => e.Type == NotificationType.Email)
                    .FirstOrDefault();
                var sms = groupedRecords
                    .Where(e => e.Type == NotificationType.Sms)
                    .FirstOrDefault();
                var viber = groupedRecords
                    .Where(e => e.Type == NotificationType.Viber)
                    .FirstOrDefault();

                // emails
                var row = worksheetPart.Worksheet.AppendRelativeRow();

                row.AppendRelativeInlineStringCell(
                    text: emails?.Type.ToString());

                row.AppendRelativeNumberCell(
                    number: emails?.Sent ?? 0);

                row.AppendRelativeNumberCell(
                    number: emails?.Error ?? 0);

                row.AppendRelativeNumberCell(
                    number: emails?.Total ?? 0);

                // sms
                row = worksheetPart.Worksheet.AppendRelativeRow();

                row.AppendRelativeInlineStringCell(
                    text: sms?.Type.ToString());

                row.AppendRelativeNumberCell(
                    number: sms?.Sent ?? 0);

                row.AppendRelativeNumberCell(
                    number: sms?.Error ?? 0);

                row.AppendRelativeNumberCell(
                    number: sms?.Total ?? 0);

                // viber
                row = worksheetPart.Worksheet.AppendRelativeRow();

                row.AppendRelativeInlineStringCell(
                    text: viber?.Type.ToString());

                row.AppendRelativeNumberCell(
                    number: viber?.Sent ?? 0);

                row.AppendRelativeNumberCell(
                    number: viber?.Error ?? 0);

                row.AppendRelativeNumberCell(
                    number: viber?.Total ?? 0);
            }

            var emptyRow = worksheetPart.Worksheet.AppendRelativeRow();

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
                    GetNotificationsReportResponse.Types.NotificationsMessage? email = report.Result
                        .FirstOrDefault(x => x.Date.ToLocalDateTime().Day == day.Day
                            && x.Date.ToLocalDateTime().Month == day.Month
                            && x.Type == NotificationType.Email);

                    var emailRow = worksheetPart.Worksheet.AppendRelativeRow();

                    emailRow.AppendRelativeInlineStringCell(
                        text: day.Date.ToString("dd/MM/yyyy"));

                    emailRow.AppendRelativeInlineStringCell(
                        text: email?.Type.ToString());

                    emailRow.AppendRelativeNumberCell(
                        number: email?.Sent ?? 0);

                    emailRow.AppendRelativeNumberCell(
                        number: email?.Error ?? 0);

                    emailRow.AppendRelativeNumberCell(
                        number: (email?.Error ?? 0) + (email?.Sent ?? 0));

                    GetNotificationsReportResponse.Types.NotificationsMessage? sms = report.Result
                        .FirstOrDefault(x => x.Date.ToLocalDateTime().Day == day.Day
                            && x.Date.ToLocalDateTime().Month == day.Month
                            && x.Type == NotificationType.Sms);

                    var smsRow = worksheetPart.Worksheet.AppendRelativeRow();

                    smsRow.AppendRelativeInlineStringCell(
                        text: day.Date.ToString("dd/MM/yyyy"));

                    smsRow.AppendRelativeInlineStringCell(
                        text: sms?.Type.ToString());

                    smsRow.AppendRelativeNumberCell(
                        number: sms?.Sent ?? 0);

                    smsRow.AppendRelativeNumberCell(
                        number: sms?.Error ?? 0);

                    smsRow.AppendRelativeNumberCell(
                        number: (sms?.Sent ?? 0) + (sms?.Error ?? 0));

                    GetNotificationsReportResponse.Types.NotificationsMessage? viber = report.Result
                        .FirstOrDefault(x => x.Date.ToLocalDateTime().Day == day.Day
                            && x.Date.ToLocalDateTime().Month == day.Month
                            && x.Type == NotificationType.Viber);

                    var viberRow = worksheetPart.Worksheet.AppendRelativeRow();

                    viberRow.AppendRelativeInlineStringCell(
                        text: day.Date.ToString("dd/MM/yyyy"));

                    viberRow.AppendRelativeInlineStringCell(
                        text: viber?.Type.ToString());

                    viberRow.AppendRelativeNumberCell(
                       number: viber?.Sent ?? 0);

                    viberRow.AppendRelativeNumberCell(
                        number: viber?.Error ?? 0);

                    viberRow.AppendRelativeNumberCell(
                        number: (viber?.Sent ?? 0) + (viber?.Error ?? 0));
                }
            }

            worksheetPart.Worksheet.Finalize();
            document.Close();

            excelStream.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "notification_report.xlsx"
            };
        }

        public async Task<ActionResult<object>> ExportTimestampsAsync(
            [FromQuery, Required][DateTimeModelBinder] DateTime fromDate,
            [FromQuery, Required][DateTimeModelBinder] DateTime toDate,
            [FromQuery, Required][DateTimeModelBinder] DateTime? requestDate,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            requestDate ??= DateTime.Now;

            int currentUserId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserId();

            GetTimestampsReportResponse report =
                await this.adminClient.GetTimestampsReportAsync(
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
                FileDownloadName = "timestamp_report.xlsx"
            };
        }

        public async Task<ActionResult<object>> ExportProfilesAsync(
            [FromQuery] string? identifier,
            [FromQuery] string? nameEmailPhone,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            int currentUserId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserId();

            GetProfileListResponse report =
                await this.adminClient.GetProfileListAsync(
                    new GetProfileListRequest
                    {
                        AdminUserId = currentUserId,
                        Identifier = identifier,
                        NameEmailPhone = nameEmailPhone,
                        Offset = 0,
                        Limit = MaxRows,
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
                    "Имейл",
                    "Идентификатор",
                    "Статус",
                    "Целева група",
                };

            var titleRow = worksheetPart.Worksheet.AppendRelativeRow();

            titleRow
                .AppendRelativeInlineStringCell(
                    text: "Справка списък профили",
                    styleIndex: titleTableCellFormat);
            worksheetPart.Worksheet
                .AppendMergeCell($"A{titleRow.RowIndex}:" +
                    $"{OpenXmlExtensions.ColumnIdToColumnIndex(headerRowColumnTitles.Length - 1)}{titleRow.RowIndex}");

            double primaryColWidth = 13;
            worksheetPart.Worksheet.GetColumns()
                .AppendCustomWidthColumn(1, 1, primaryColWidth * 8)
                .AppendCustomWidthColumn(2, 2, primaryColWidth * 2)
                .AppendCustomWidthColumn(3, 3, primaryColWidth * 2)
                .AppendCustomWidthColumn(4, 4, primaryColWidth)
                .AppendCustomWidthColumn(5, 5, primaryColWidth * 2);

            var headerRow = worksheetPart.Worksheet.AppendRelativeRow();
            foreach (var columnTitle in headerRowColumnTitles)
            {
                headerRow
                    .AppendRelativeInlineStringCell(
                        text: $"{columnTitle}",
                        styleIndex: headerTableCell);
            }

            string activeStatus = "Активен";
            string inActiveStatus = "Неактивен";

            foreach (var profile in report.Result)
            {
                var row = worksheetPart.Worksheet.AppendRelativeRow();

                row.AppendRelativeInlineStringCell(
                    text: profile.ElectronicSubjectName);
                row.AppendRelativeInlineStringCell(
                    text: profile.Email);
                row.AppendRelativeInlineStringCell(
                    text: profile.Identifier);
                row.AppendRelativeInlineStringCell(
                    text: profile.IsActivated ? activeStatus : inActiveStatus);
                row.AppendRelativeInlineStringCell(
                    text: profile.TargetGroupName);
            }

            worksheetPart.Worksheet.Finalize();
            document.Close();

            excelStream.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "profiles_report.xlsx"
            };
        }

        public async Task<ActionResult<object>> ExportTicketsAsync(
            [FromQuery, Required][DateTimeModelBinder] DateTime @from,
            [FromQuery, Required][DateTimeModelBinder] DateTime to,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            int currentUserId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserId();

            GetTicketsReportResponse report =
                await this.adminClient.GetTicketsReportAsync(
                    new GetTicketsReportRequest()
                    {
                        AdminUserId = currentUserId,
                        From = @from.ToTimestamp(),
                        To = to.ToTimestamp(),
                    },
                    cancellationToken: ct);

            // do not dispose the stream, this will be done by the FileStreamResult
            MemoryStream excelStream = new();
            using SpreadsheetDocument document = SpreadsheetDocument.Create(excelStream, SpreadsheetDocumentType.Workbook, autoSave: true);

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

            Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>()!;
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
                    "Дата",
                    "Фиш - ФЛ",
                    "НП - ФЛ",
                    "Фиш - ЮЛ",
                    "НП - ЮЛ",
                    "Връчен в ССЕВ",
                    "Връчени външно",
                    "Анулирани",
                    "Имейл",
                    "Телефон",
                    "Връчени Фиш - ФЛ",
                    "Връчени НП - ФЛ",
                    "Връчени Фиш - ЮЛ",
                    "Връчени НП - ЮЛ",
                    "Активни профили",
                    "Пасивни профили",
                };

            Row titleRow = worksheetPart.Worksheet.AppendRelativeRow();

            titleRow
                .AppendRelativeInlineStringCell(
                    text: $"Справка е-фишове от {@from:dd-MM-yyyy} до {to:dd-MM-yyyy}",
                    styleIndex: titleTableCellFormat);
            worksheetPart.Worksheet
                .AppendMergeCell($"A{titleRow.RowIndex}:" +
                    $"{OpenXmlExtensions.ColumnIdToColumnIndex(headerRowColumnTitles.Length - 1)}{titleRow.RowIndex}");

            Row headerRow = worksheetPart.Worksheet.AppendRelativeRow();

            foreach (var columnTitle in headerRowColumnTitles)
            {
                headerRow
                    .AppendRelativeInlineStringCell(
                        text: $"{columnTitle}",
                        styleIndex: headerTableCell);
            }

            foreach (var ticketStat in report.Result)
            {
                Row row = worksheetPart.Worksheet.AppendRelativeRow();

                row.AppendRelativeDateCell(
                    date: ticketStat.TicketStatDate.ToLocalDateTime(),
                    styleIndex: dateCell);

                row.AppendRelativeNumberCell(
                    number: ticketStat.ReceivedTicketIndividuals);

                row.AppendRelativeNumberCell(
                    number: ticketStat.ReceivedPenalDecreeIndividuals);

                row.AppendRelativeNumberCell(
                    number: ticketStat.ReceivedTicketLegalEntites);

                row.AppendRelativeNumberCell(
                    number: ticketStat.ReceivedPenalDecreeLegalEntites);


                row.AppendRelativeNumberCell(
                    number: ticketStat.InternalServed);

                row.AppendRelativeNumberCell(
                    number: ticketStat.ExternalServed);

                row.AppendRelativeNumberCell(
                    number: ticketStat.Annulled);


                row.AppendRelativeNumberCell(
                    number: ticketStat.EmailNotifications);

                row.AppendRelativeNumberCell(
                    number: ticketStat.PhoneNotifications);


                row.AppendRelativeNumberCell(
                    number: ticketStat.DeliveredTicketIndividuals);

                row.AppendRelativeNumberCell(
                    number: ticketStat.DeliveredPenalDecreeIndividuals);

                row.AppendRelativeNumberCell(
                    number: ticketStat.DeliveredTicketLegalEntites);

                row.AppendRelativeNumberCell(
                    number: ticketStat.DeliveredPenalDecreeLegalEntites);


                row.AppendRelativeNumberCell(
                    number: ticketStat.SentToActiveProfiles);

                row.AppendRelativeNumberCell(
                    number: ticketStat.SentToPassiveProfiles);
            }

            worksheetPart.Worksheet.Finalize();
            document.Close();

            excelStream.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "tickets_report.xlsx"
            };
        }

        protected IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
        {
            for (DateTime day = from.Date; day.Date <= to.Date; day = day.AddDays(1))
            {
                yield return day;
            }
        }
    }
}
