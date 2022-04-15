using System;
using System.Runtime.Serialization;

namespace ED.Domain
{
    [Serializable]
    public class OrnException : Exception
    {
        public OrnException()
        {
        }

        public OrnException(string message)
            : base(message)
        {
        }

        public OrnException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected OrnException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
