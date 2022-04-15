using System;
using System.Runtime.Serialization;

namespace ED.Blobs
{
    [Serializable]
    public class PdfServicesClientException : Exception
    {
        public PdfServicesClientException()
        {
        }
        
        public PdfServicesClientException(string message)
            : base(message)
        { 
        }

        public PdfServicesClientException(string message, Exception inner)
            : base(message, inner)
        {
        }
        
        protected PdfServicesClientException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
