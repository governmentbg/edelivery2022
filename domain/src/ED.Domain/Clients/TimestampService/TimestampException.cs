using System;
using System.Runtime.Serialization;

namespace ED.Domain
{
    [Serializable]
    public class TimestampException : Exception
    {
        public TimestampException()
        {
        }

        public TimestampException(string message)
            : base(message)
        {
        }

        public TimestampException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected TimestampException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
