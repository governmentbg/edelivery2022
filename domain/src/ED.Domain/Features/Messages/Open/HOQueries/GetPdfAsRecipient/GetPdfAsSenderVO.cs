namespace ED.Domain
{
    public partial interface IMessageOpenHORepository
    {
        public record GetPdfAsRecipientVO(
            string FileName,
            string ContentType,
            byte[] Content);
    }
}
