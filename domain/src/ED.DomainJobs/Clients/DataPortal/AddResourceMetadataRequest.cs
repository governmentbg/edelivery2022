#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace ED.DomainJobs
{
    public class AddResourceMetadataRequest
    {
        public string api_key { get; set; } = null!;

        public string dataset_uri { get; set; } = null!;

        public AddResourceMetadataRequestData data { get; set; } = null!;

    }

    public class AddResourceMetadataRequestData
    {
        public string? locale { get; set; }

        public string name { get; set; } = null!;

        public string file_format { get; set; } = null!;

        public int type { get; set; }
    }
}
