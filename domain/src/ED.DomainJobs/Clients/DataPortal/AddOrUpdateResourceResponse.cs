#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace ED.DomainJobs
{
    public class AddOrUpdateResourceResponse
    {
        public bool success { get; set; }

        public string message { get; set; } = null!;
    }
}
