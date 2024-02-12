using System;
using System.Runtime.Serialization;

namespace ED.DomainJobs
{
    [Serializable]
    public class DataPortalException : Exception
    {
        public DataPortalException()
        {
        }

        public DataPortalException(string message)
            : base(message)
        {
        }

        public DataPortalException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected DataPortalException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
