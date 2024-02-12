#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace ED.DomainJobs
{
    public class AddResourceMetadataResponse
    {
        public bool success { get; set; }

        public AddResourceMetadataResponseData data { get; set; } = null!;
    }

    public class AddResourceMetadataResponseData
    {
        public string uri { get; set; } = null!;
    }
}
