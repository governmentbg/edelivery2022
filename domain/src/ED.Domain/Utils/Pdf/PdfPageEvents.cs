using System.IO;
using System.Reflection;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.FileProviders;

namespace ED.Domain
{
    public class PdfPageEvents : PdfPageEventHelper
    {
        private static Image Image = InitImage();

        private static Image InitImage()
        {
            using var logoStream = new EmbeddedFileProvider(
                   typeof(PdfOptions).GetTypeInfo().Assembly
               )
               .GetFileInfo("EmbeddedResources/logo.png")
               .CreateReadStream();

            using var logoMemoryStream = new MemoryStream();

            logoStream.CopyTo(logoMemoryStream);

            Image img = Image.GetInstance(logoMemoryStream.ToArray(), false);
            img.ScaleAbsolute(72, 56);

            return img;
        }

        private readonly string title;

        private PdfPTable? table;

        public PdfPageEvents(string title)
        {
            this.title = title;
        }

        public override void OnOpenDocument(
            PdfWriter writer,
            Document document)
        {
            Font font = new(PdfWriterUtils.BaseFont, 14, Font.NORMAL);

            this.table = new(2);
            this.table.SetWidths(new float[] { 1, 2 });

            this.table.LockedWidth = true;
            this.table.TotalWidth = PageSize.A4.Width - (2 * PdfWriterUtils.SideMargin);

            PdfPCell logoCell = new(Image)
            {
                HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
                VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                Padding = 5,
                Border = Rectangle.BOTTOM_BORDER,
                BorderWidthBottom = 1
            };

            Paragraph paragraph = new(this.title, font);

            PdfPCell titleCell = new();
            titleCell.AddElement(paragraph);
            titleCell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
            titleCell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
            titleCell.Padding = 5;
            titleCell.Border = Rectangle.BOTTOM_BORDER;
            titleCell.BorderWidthBottom = 1;

            this.table.AddCell(logoCell);
            this.table.AddCell(titleCell);
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            this.table!.WriteSelectedRows(
                0,
                -1,
                PdfWriterUtils.SideMargin,
                document.Top + PdfWriterUtils.TopMargin - 10f,
                writer.DirectContent);
        }
    }
}
