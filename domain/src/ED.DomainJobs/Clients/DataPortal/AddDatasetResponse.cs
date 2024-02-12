#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace ED.DomainJobs
{
    public class AddDatasetResponse
    {
        public bool success { get; set; }

        public string uri { get; set; } = null!;
    }
}
