#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace ED.DomainJobs
{
    public class AddDatasetRequest
    {
        public string api_key { get; set; } = null!;

        public AddDatasetRequestData data { get; set; } = null!;

    }

    public class AddDatasetRequestData
    {
        public int org_id { get; set; }

        public string? locale { get; set; }

        public string name { get; set; } = null!;

        public int category_id { get; set; }

        public string? description { get; set; }

        public int visibility { get; set; }
    }
}
