using System;
using System.Runtime.Serialization;

namespace ED.DomainJobs
{
    [Serializable]
    public class LinkMobilityException : Exception
    {
        public LinkMobilityException()
        {
        }

        public LinkMobilityException(string message)
            : base(message)
        {
        }

        public LinkMobilityException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected LinkMobilityException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
