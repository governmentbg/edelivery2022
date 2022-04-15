namespace ED.Domain
{
    public partial interface IMessageOpenHORepository
    {
        public record GetPdfAsSenderVO(
            string FileName,
            string ContentType,
            byte[] Content);
    }
}
