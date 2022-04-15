using System;
using System.Runtime.Serialization;

namespace ED.DomainJobs
{
    [Serializable]
    public class PushNotificationException : Exception
    {
        public PushNotificationException()
        {
        }

        public PushNotificationException(string message)
            : base(message)
        {
        }

        public PushNotificationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected PushNotificationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
