using System;

namespace EDelivery.SEOS.As4RestService
{
    public class As4RestClientException : Exception
    {
        public string Status { get; private set; }

        public int StatusCode { get; private set; }

        public string Body { get; private set; }

        public As4RestClientException(
            string message,
            string status,
            int statusCode,
            string body,
            Exception innerException)
            : base(message, innerException)
        {
            this.Status = status;
            this.StatusCode = statusCode;
            this.Body = body;
        }

        public override string ToString()
            => $"HTTP Response: {this.StatusCode} {this.Status}\n\n{this.Body}\n\n{base.Message}";
    }
}
