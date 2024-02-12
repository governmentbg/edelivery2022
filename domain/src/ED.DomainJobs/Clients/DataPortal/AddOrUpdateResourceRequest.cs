#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace ED.DomainJobs
{
    public class AddOrUpdateResourceRequest
    {
        public string api_key { get; set; } = null!;

        public string resource_uri { get; set; } = null!;

        public string extension_format { get; set; } = null!;

        public object data { get; set; } = null!;
    }
}
