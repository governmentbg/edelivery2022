using System.Runtime.Serialization;
using System;

namespace ED.Domain
{
    [Serializable]
    public class EsbException : Exception
    {
        public EsbException()
        {
        }

        public EsbException(string message)
            : base(message)
        {
        }

        public EsbException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected EsbException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
