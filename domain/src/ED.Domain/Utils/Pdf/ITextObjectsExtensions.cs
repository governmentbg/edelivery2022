using iTextSharp.text.pdf;

namespace ED.Domain
{
    public static class ITextObjectsExtensions
    {
        public static void AddRow(this PdfPTable table, PdfPCell?[] cells)
        {
            foreach (var cell in cells)
            {
                if (cell != null)
                {
                    table.AddCell(cell);
                }
            }
        }
    }
}
