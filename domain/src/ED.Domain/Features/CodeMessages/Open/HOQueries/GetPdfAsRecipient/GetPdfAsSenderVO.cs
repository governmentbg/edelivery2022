namespace ED.Domain
{
    public partial interface ICodeMessageOpenHORepository
    {
        public record GetPdfAsRecipientVO(
            string FileName,
            string ContentType,
            byte[] Content);
    }
}
