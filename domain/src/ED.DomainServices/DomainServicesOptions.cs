using System;

namespace ED.DomainServices
{
    public class DomainServicesOptions
    {
        public string[]? GrpcServiceHosts { get; set; }

        public bool EnableGrpcWeb { get; set; }

        public TimeSpan? DelayGrpcMethodsForTesting { get; set; }

        public int? MaxReceiveMessageSize { get; set; }
    }
}
