namespace ED.Domain
{
    public partial interface IJobsMessagesOpenQueryRepository
    {
        public record GetTemplateContentVO(
            int TemplateId,
            string Content);
    }
}
