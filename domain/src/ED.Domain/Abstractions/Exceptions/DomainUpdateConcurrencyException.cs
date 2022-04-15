namespace ED.Domain
{
    public class DomainUpdateConcurrencyException : DomainException
    {
        public DomainUpdateConcurrencyException()
            : base("Entity already modified")
        {
        }

        public DomainUpdateConcurrencyException(string message)
            : base(message)
        {
        }
    }
}
