using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        private static Image CheckboxTrueImage = InitCheckboxImage("cb-true.png");
        private static Image CheckboxFalseImage = InitCheckboxImage("cb-false.png");

        private static Image InitCheckboxImage(string fileName)
        {
            using var logoStream = new EmbeddedFileProvider(
                   typeof(PdfOptions).GetTypeInfo().Assembly
               )
               .GetFileInfo($"EmbeddedResources/{fileName}")
               .CreateReadStream();

            using var logoMemoryStream = new MemoryStream();

            logoStream.CopyTo(logoMemoryStream);

            Image img = Image.GetInstance(logoMemoryStream.ToArray(), false);
            img.ScaleAbsolute(15, 15);

            return img;
        }

        public static BaseFont BaseFont => baseFont;

        private static BaseColor formColor = new(221, 228, 255, 255);

        private static Font normalFont = new(BaseFont, 12, Font.NORMAL);
        private static Font smallFont = new(BaseFont, 6, Font.NORMAL);

        private static PdfPCell GetTd(
            Phrase phrase,
            BaseColor? backgroundColor,
            int colspan)
        {
            PdfPCell cell = new(phrase)
            {
                Colspan = colspan,
                Border = PdfPCell.NO_BORDER,
                PaddingBottom = 3f
            };

            if (backgroundColor != null)
            {
                cell.BackgroundColor = backgroundColor;
            }

            return cell;
        }

        private static PdfPCell GetTd(
            Image image,
            BaseColor? backgroundColor,
            int colspan)
        {
            PdfPCell cell = new(image)
            {
                Colspan = colspan,
                Border = PdfPCell.NO_BORDER,
                PaddingBottom = 3f
            };

            if (backgroundColor != null)
            {
                cell.BackgroundColor = backgroundColor;
            }

            return cell;
        }

        private static Phrase GetDatePhrase(DateTime dateSent, string hash)
        {
            Phrase p = new($"{dateSent:dd.MM.yyyy HH:mm:ss}", normalFont)
            {
                "\n",
                new Chunk($"SHA-256: {hash}", smallFont)
            };

            return p;
        }

        record AttachementDO(string FileName, string FileHash);

        private static Phrase GetAttachmentsPhrase(
            AttachementDO[] blobs)
        {
            Phrase p = new();

            for (int i = 0; i < blobs.Length; i++)
            {
                p.Add(new Chunk($"{blobs[i].FileName} \n", normalFont));
                p.Add(new Chunk($"{blobs[i].FileHash} \n", smallFont));
            }

            return p;
        }

        private static PdfPCell GetEmpty(float height, int colspan)
        {
            PdfPCell cell = new(new Phrase(string.Empty, normalFont))
            {
                Border = PdfPCell.NO_BORDER,
                FixedHeight = height,
                Colspan = colspan
            };

            return cell;
        }

        private static PdfPCell GetRecipientTableTd(
            (string Name, DateTime? DateReceived, string? MessageSummaryHash)[] recipients)
        {
            PdfPTable table = new(2)
            {
                LockedWidth = false,
                WidthPercentage = 100
            };
            table.SetWidths(new float[] { 1, 1 });
            table.SplitLate = false;

            foreach (var recipient in recipients)
            {
                PdfPCell nameCell = new(new Phrase(recipient.Name, normalFont))
                {
                    Rowspan = 2,
                    Border = PdfPCell.BOTTOM_BORDER | PdfPCell.RIGHT_BORDER,
                    BorderColor = BaseColor.WHITE
                };

                table.AddCell(nameCell);

                if (recipient.DateReceived.HasValue)
                {
                    PdfPCell dateCell = new(
                        new Phrase(
                            $" {recipient.DateReceived.Value:dd.MM.yyyy HH:mm:ss}",
                            normalFont))
                    {
                        Border = PdfPCell.NO_BORDER,
                        BorderColor = BaseColor.WHITE,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        HorizontalAlignment = PdfPCell.ALIGN_LEFT
                    };

                    PdfPCell hashCell = new(
                        new Phrase($"SHA-256: {recipient.MessageSummaryHash!}", smallFont))
                    {
                        Border = PdfPCell.BOTTOM_BORDER,
                        BorderColor = BaseColor.WHITE,
                        VerticalAlignment = PdfPCell.ALIGN_BOTTOM,
                        HorizontalAlignment = PdfPCell.ALIGN_RIGHT
                    };

                    table.AddCell(dateCell);
                    table.AddCell(hashCell);
                }
                else
                {
                    PdfPCell dateCell = new()
                    {
                        Border = PdfPCell.NO_BORDER
                    };

                    PdfPCell hashCell = new()
                    {
                        Border = PdfPCell.BOTTOM_BORDER,
                        BorderColor = BaseColor.WHITE
                    };

                    table.AddCell(dateCell);
                    table.AddCell(hashCell);
                }
            }

            PdfPCell cell = new(table)
            {
                Border = PdfPCell.NO_BORDER,
                BackgroundColor = new BaseColor(221, 228, 255, 255)
            };

            return cell;
        }

        private static PdfPCell?[] GetRow(Phrase label, Phrase? value)
        {
            PdfPCell?[] cells = new PdfPCell?[3];

            int colspan = label.Content.Length > 12 || value == null ? 2 : 1;

            cells[0] = GetTd(label, null, colspan);

            if (value != null && value.Content.Length == 0)
            {
                value = new Phrase(" ");
            } 
            cells[1] = value != null
                ? GetTd(value, formColor, colspan)
                : null;

            cells[2] = value != null
                ? GetEmpty(10, 2)
                : null;

            return cells;
        }

        private static PdfPCell?[] GetCheckboxRow(Phrase label, bool value)
        {
            PdfPCell?[] cells = new PdfPCell?[3];

            cells[0] = GetTd(new Phrase(string.Empty, normalFont), null, 1);
            cells[1] = GetCheckboxTableTd(label, value);

            cells[2] = GetEmpty(10, 2);

            return cells;
        }

        private static PdfPCell GetCheckboxTableTd(Phrase label, bool value)
        {
            PdfPTable table = new(2)
            {
                LockedWidth = false,
                WidthPercentage = 100
            };
            table.SetWidths(new float[] { 1, 25 });
            table.SplitLate = false;

            PdfPCell valueCell = GetTd(value ? CheckboxTrueImage : CheckboxFalseImage, null, 1);
            PdfPCell labelCell = GetTd(label, null, 1);

            table.AddCell(valueCell);
            table.AddCell(labelCell);

            PdfPCell cell = new(table)
            {
                Border = PdfPCell.NO_BORDER,
            };

            return cell;
        }

        public static MemoryStream CreatePdf(
            MemoryStream stream,
            PdfOptions options,
            string senderProfileName,
            DateTime dateSent,
            string messageSummaryHash,
            (string Name, DateTime? DateReceived, string? MessageSummaryHash)[] recipients,
            string subject,
            string? rnu,
            string templateJson,
            string messageBody)
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

            PdfPTable table = new(2)
            {
                LockedWidth = true,
                TotalWidth = PageSize.A4.Width - (2 * SideMargin)
            };
            table.SetWidths(new float[] { 1, 5 });
            table.SplitLate = false;

            PdfPCell?[] senderRow = GetRow(
                new Phrase("Връчител", normalFont),
                new Phrase(senderProfileName, normalFont));
            table.AddRow(senderRow);

            PdfPCell?[] sentRow = GetRow(
                new Phrase("Изпратено на", normalFont),
                GetDatePhrase(dateSent, messageSummaryHash));
            table.AddRow(sentRow);

            table.AddCell(GetTd(new Phrase("Получатели", normalFont), null, 1));
            table.AddCell(GetRecipientTableTd(recipients));
            table.AddCell(GetEmpty(10, 2));

            //PdfPCell?[] rnuRow = GetRow(
            //    new Phrase("Референтен номер на услуга (РНУ)", normalFont),
            //    new Phrase(rnu ?? string.Empty, normalFont));
            //table.AddRow(rnuRow);

            PdfPCell?[] subjectRow = GetRow(
                new Phrase("Заглавие", normalFont),
                new Phrase(subject, normalFont));
            table.AddRow(subjectRow);

            Dictionary<Guid, object> valuesDictionary =
                JsonConvert.DeserializeObject<Dictionary<Guid, object>>(messageBody)!;

            IList<BaseComponent> templateComponents =
                TemplateSerializationHelper.DeserializeModel(templateJson);

            foreach (BaseComponent component in templateComponents)
            {
                object? value = null;
                if (valuesDictionary.ContainsKey(component.Id))
                {
                    value = valuesDictionary[component.Id];
                }

                switch (component.Type)
                {
                    case ComponentType.checkbox:
                        CheckboxComponent checkboxComponent = (CheckboxComponent)component;
                        bool boolValue = Convert.ToBoolean(value);

                        PdfPCell?[] rcb = GetCheckboxRow(
                            new Phrase(component.Label, normalFont),
                            boolValue);

                        table.AddRow(rcb);
                        break;
                    case ComponentType.file:
                        AttachementDO[] blobs = value != null
                            ? ((JArray)value).ToObject<AttachementDO[]>()
                                ?? Array.Empty<AttachementDO>()
                            : Array.Empty<AttachementDO>();

                        PdfPCell?[] rf = GetRow(
                                new Phrase(component.Label, normalFont),
                                GetAttachmentsPhrase(blobs));

                        table.AddRow(rf);
                        break;
                    case ComponentType.hidden:
                        break;
                    case ComponentType.markdown:
                        MarkdownComponent markdownComponent = (MarkdownComponent)component;
                        if (!string.IsNullOrEmpty(markdownComponent.PdfValue))
                        {
                            PdfPCell?[] rmd = GetRow(
                                new Phrase(markdownComponent.PdfValue, normalFont),
                                null);

                            table.AddRow(rmd);
                        }
                        break;
                    case ComponentType.select:
                    case ComponentType.datetime:
                    case ComponentType.textfield:
                    case ComponentType.textarea:
                        PdfPCell?[] r = GetRow(
                            new Phrase(component.Label, normalFont),
                            new Phrase(value?.ToString() ?? string.Empty, normalFont));

                        table.AddRow(r);
                        break;
                    default:
                        throw new NotImplementedException();
                }
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
