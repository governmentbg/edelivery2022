using System;
using System.Runtime.Serialization;

namespace ED.DomainJobs
{
    [Serializable]
    public class InfosystemsException : Exception
    {
        public InfosystemsException()
        {
        }

        public InfosystemsException(string message)
            : base(message)
        {
        }

        public InfosystemsException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InfosystemsException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
