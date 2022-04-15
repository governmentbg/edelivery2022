using System;

namespace ED.Domain
{
    public class DomainValidationException : DomainException
    {
        public DomainValidationException(string message)
            : base(message)
        {
        }
        public DomainValidationException(string[] errors, string[] errorMessages)
            : this("Domain validation failed")
        {
            this.Errors = errors;
            this.ErrorMessages = errorMessages;
        }

        public string[] Errors { get; init; } = Array.Empty<string>();
        public string[] ErrorMessages { get; init; } = Array.Empty<string>();
    }
}
