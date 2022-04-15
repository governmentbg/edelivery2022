using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.FileProviders;

namespace ED.Domain
{
    public static class PdfWriterUtils
    {
        public const string SignatureField = "Signature";

        public const float SideMargin = 30f;
        public const float TopMargin = 110f;
        public const float BottomMargin = 50f;

        private static BaseFont baseFont = InitBaseFont();

        private static BaseFont InitBaseFont()
        {
            using var fontStream = new EmbeddedFileProvider(
                   typeof(PdfOptions).GetTypeInfo().Assembly
               )
               .GetFileInfo("EmbeddedResources/arial.ttf")
               .CreateReadStream();

            using var fontMemoryStream = new MemoryStream();

            fontStream.CopyTo(fontMemoryStream);

            return BaseFont.CreateFont(
                "arial.ttf",
                BaseFont.IDENTITY_H,
                BaseFont.EMBEDDED,
                true,
                fontMemoryStream.ToArray(),
                null);
        }

        public static BaseFont BaseFont => baseFont;

        private static BaseColor formColor = new(221, 228, 255, 255);

        private static Font normalFont = new(BaseFont, 12, Font.NORMAL);
        private static Font smallFont = new(BaseFont, 6, Font.NORMAL);

        private static PdfPCell GetTd(
            string title,
            BaseColor? backgroundColor = null)
        {
            PdfPCell cell = new(new Phrase(title, normalFont));
            cell.Border = PdfPCell.NO_BORDER;
            cell.PaddingBottom = 3f;

            if (backgroundColor != null)
            {
                cell.BackgroundColor = backgroundColor;
            }

            return cell;
        }

        private static PdfPCell GetDateTd(DateTime dateSent, string hash)
        {
            Phrase p = new($"{dateSent:dd.MM.yyyy HH:mm:ss}", normalFont);
            p.Add("\n");
            p.Add(new Chunk($"SHA-256: {hash}", smallFont));

            PdfPCell cell = new(p);
            cell.Border = PdfPCell.NO_BORDER;
            cell.BackgroundColor = formColor;
            cell.PaddingBottom = 3f;

            return cell;
        }

        private static PdfPCell GetAttachmentsTd(
            (string FileName, string Hash, string HashAlgorithm, string Size)[] blobs)
        {
            Phrase p = new();

            for (int i = 0; i < blobs.Length; i++)
            {
                string fileName = $"{blobs[i].FileName} {blobs[i].Size}";
                string hash = $"{blobs[i].HashAlgorithm}: {blobs[i].Hash}";

                p.Add(new Chunk($"{fileName} ", normalFont));
                p.Add(new Chunk("\n"));
                p.Add(new Chunk(hash, smallFont));
                p.Add(new Chunk("\n"));
            }

            PdfPCell cell = new(p);
            cell.Border = PdfPCell.NO_BORDER;
            cell.BackgroundColor = formColor;
            cell.PaddingBottom = 3f;

            return cell;
        }

        private static PdfPCell GetEmpty(float height, int colspan)
        {
            PdfPCell cell = new(new Phrase(string.Empty, normalFont));
            cell.Border = PdfPCell.NO_BORDER;
            cell.FixedHeight = height;
            cell.Colspan = colspan;

            return cell;
        }

        private static PdfPCell GetRecipientTableTd(
            (string Name, DateTime? DateReceived, string? MessageSummaryHash)[] recipients)
        {
            PdfPTable table = new(2);
            table.LockedWidth = false;
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 1, 1 });
            table.SplitLate = false;

            foreach (var recipient in recipients)
            {
                PdfPCell nameCell = new(new Phrase(recipient.Name, normalFont));
                nameCell.Rowspan = 2;
                nameCell.Border = PdfPCell.BOTTOM_BORDER | PdfPCell.RIGHT_BORDER;
                nameCell.BorderColor = BaseColor.WHITE;

                table.AddCell(nameCell);

                if (recipient.DateReceived.HasValue)
                {
                    PdfPCell dateCell = new(
                        new Phrase(
                            $" {recipient.DateReceived.Value:dd.MM.yyyy HH:mm:ss}",
                            normalFont));
                    dateCell.Border = PdfPCell.NO_BORDER;
                    dateCell.BorderColor = BaseColor.WHITE;
                    dateCell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    dateCell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;

                    PdfPCell hashCell = new(
                        new Phrase($"SHA-256: {recipient.MessageSummaryHash!}", smallFont));
                    hashCell.Border = PdfPCell.BOTTOM_BORDER;
                    hashCell.BorderColor = BaseColor.WHITE;
                    hashCell.VerticalAlignment = PdfPCell.ALIGN_BOTTOM;
                    hashCell.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;

                    table.AddCell(dateCell);
                    table.AddCell(hashCell);
                }
                else
                {
                    PdfPCell dateCell = new();
                    dateCell.Border = PdfPCell.NO_BORDER;

                    PdfPCell hashCell = new();
                    hashCell.Border = PdfPCell.BOTTOM_BORDER;
                    hashCell.BorderColor = BaseColor.WHITE;

                    table.AddCell(dateCell);
                    table.AddCell(hashCell);
                }
            }

            PdfPCell cell = new(table);
            cell.Border = PdfPCell.NO_BORDER;
            cell.BackgroundColor = new BaseColor(221, 228, 255, 255);

            return cell;
        }

        public static MemoryStream CreatePdfAsSender(
            MemoryStream stream,
            PdfOptions options,
            string senderProfileName,
            DateTime dateSent,
            string messageSummaryHash,
            (string Name, DateTime? DateReceived, string? MessageSummaryHash)[] recipients,
            string subject,
            Dictionary<string, string> body,
            (string FileName, string Hash, string HashAlgorithm, string Size)[] blobs)
        {
            using Document document = new(
                PageSize.A4,
                SideMargin,
                SideMargin,
                TopMargin,
                BottomMargin);

            using PdfWriter writer = PdfWriter.GetInstance(document, stream);
            writer.CloseStream = false;

            writer.PageEvent = new PdfPageEvents(options.Title!);

            document.Open();

            document.AddTitle(options.MetaTitle!);
            document.AddAuthor(options.MetaAuthor!);
            document.AddSubject(options.MetaSubject!);
            document.AddKeywords(options.MetaKeywords!);
            document.AddCreator(options.MetaCreator!);

            PdfPTable table = new(2);
            table.LockedWidth = true;
            table.TotalWidth = PageSize.A4.Width - (2 * SideMargin);
            table.SetWidths(new float[] { 1, 5 });
            table.SplitLate = false;

            table.AddCell(GetTd("Връчител"));
            table.AddCell(GetTd(senderProfileName, formColor));
            table.AddCell(GetEmpty(10, 2));

            table.AddCell(GetTd("Изпратено на"));
            table.AddCell(GetDateTd(dateSent, messageSummaryHash));
            table.AddCell(GetEmpty(10, 2));

            table.AddCell(GetTd("Получатели"));
            table.AddCell(GetRecipientTableTd(recipients));
            table.AddCell(GetEmpty(10, 2));

            table.AddCell(GetTd("Заглавие"));
            table.AddCell(GetTd(subject, formColor));
            table.AddCell(GetEmpty(10, 2));

            foreach (var item in body)
            {
                table.AddCell(GetTd(item.Key));
                table.AddCell(GetTd(item.Value, formColor));
                table.AddCell(GetEmpty(10, 2));
            }

            if (blobs.Any())
            {
                table.AddCell(GetTd("Документи за връчване"));
                table.AddCell(GetAttachmentsTd(blobs));
            }

            document.Add(table);

            // signature field
            var verticalPosition = writer.GetVerticalPosition(true);

            if (verticalPosition < 100 + BottomMargin)
            {
                document.NewPage();
            }

            verticalPosition = writer.GetVerticalPosition(true);

            PdfFormField field = PdfFormField.CreateSignature(writer);
            field.SetWidget(
                new Rectangle(
                    SideMargin,
                    verticalPosition - 100,
                    SideMargin + 150,
                    verticalPosition - 30),
                PdfAnnotation.HIGHLIGHT_INVERT);
            field.FieldName = SignatureField;
            field.Flags = PdfAnnotation.FLAGS_PRINT;
            field.SetPage();
            field.MKBorderColor = BaseColor.BLACK;
            field.MKBackgroundColor = BaseColor.WHITE;
            field.BorderStyle = new PdfBorderDictionary(1, PdfBorderDictionary.STYLE_UNDERLINE);
            PdfAppearance tp = PdfAppearance.CreateAppearance(writer, 100, 50);
            tp.Rectangle(0.5, 0.5, 99, 49);
            tp.Stroke();
            field.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, tp);

            writer.AddAnnotation(field);

            document.Close();
            writer.Close();

            return stream;
        }

        public static MemoryStream CreatePdfAsRecipient(
            MemoryStream stream,
            PdfOptions options,
            string senderProfileName,
            DateTime dateSent,
            string messageSummaryHash,
            (string Name, DateTime DateReceived, string MessageSummaryHash) recipient,
            string subject,
            Dictionary<string, string> body,
            (string FileName, string Hash, string HashAlgorithm, string Size)[] blobs)
        {
            using Document document = new(
                PageSize.A4,
                SideMargin,
                SideMargin,
                TopMargin,
                BottomMargin);

            using PdfWriter writer = PdfWriter.GetInstance(document, stream);
            writer.CloseStream = false;

            writer.PageEvent = new PdfPageEvents(options.Title!);

            document.Open();

            document.AddTitle(options.MetaTitle!);
            document.AddAuthor(options.MetaAuthor!);
            document.AddSubject(options.MetaSubject!);
            document.AddKeywords(options.MetaKeywords!);
            document.AddCreator(options.MetaCreator!);

            PdfPTable table = new(2);
            table.LockedWidth = true;
            table.TotalWidth = PageSize.A4.Width - (2 * SideMargin);
            table.SetWidths(new float[] { 1, 5 });
            table.SplitLate = false;

            table.AddCell(GetTd("Връчител"));
            table.AddCell(GetTd(senderProfileName, formColor));
            table.AddCell(GetEmpty(10, 2));

            table.AddCell(GetTd("Изпратено на"));
            table.AddCell(GetDateTd(dateSent, messageSummaryHash));
            table.AddCell(GetEmpty(10, 2));

            table.AddCell(GetTd("Получател"));
            table.AddCell(GetTd(recipient.Name, formColor));
            table.AddCell(GetEmpty(10, 2));

            table.AddCell(GetTd("Получено на"));
            table.AddCell(GetDateTd(recipient.DateReceived, recipient.MessageSummaryHash));
            table.AddCell(GetEmpty(10, 2));

            table.AddCell(GetTd("Заглавие"));
            table.AddCell(GetTd(subject, formColor));
            table.AddCell(GetEmpty(10, 2));

            foreach (var item in body)
            {
                table.AddCell(GetTd(item.Key));
                table.AddCell(GetTd(item.Value, formColor));
                table.AddCell(GetEmpty(10, 2));
            }

            if (blobs.Any())
            {
                table.AddCell(GetTd("Документи за връчване"));
                table.AddCell(GetAttachmentsTd(blobs));
            }

            document.Add(table);

            // signature field
            var verticalPosition = writer.GetVerticalPosition(true);

            if (verticalPosition < 100 + BottomMargin)
            {
                document.NewPage();
            }

            verticalPosition = writer.GetVerticalPosition(true);

            PdfFormField field = PdfFormField.CreateSignature(writer);
            field.SetWidget(
                new Rectangle(
                    SideMargin,
                    verticalPosition - 100,
                    SideMargin + 150,
                    verticalPosition - 30),
                PdfAnnotation.HIGHLIGHT_INVERT);
            field.FieldName = SignatureField;
            field.Flags = PdfAnnotation.FLAGS_PRINT;
            field.SetPage();
            field.MKBorderColor = BaseColor.BLACK;
            field.MKBackgroundColor = BaseColor.WHITE;
            field.BorderStyle = new PdfBorderDictionary(1, PdfBorderDictionary.STYLE_UNDERLINE);
            PdfAppearance tp = PdfAppearance.CreateAppearance(writer, 100, 50);
            tp.Rectangle(0.5, 0.5, 99, 49);
            tp.Stroke();
            field.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, tp);

            writer.AddAnnotation(field);

            document.Close();
            writer.Close();

            return stream;
        }
    }
}
