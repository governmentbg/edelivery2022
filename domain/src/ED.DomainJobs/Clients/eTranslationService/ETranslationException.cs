using System;
using System.Runtime.Serialization;

namespace ED.DomainJobs
{
    [Serializable]
    public class ETranslationException : Exception
    {
        public ETranslationException()
        {
        }

        public ETranslationException(string message)
            : base(message)
        {
        }

        public ETranslationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ETranslationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
