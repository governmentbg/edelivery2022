namespace ED.Domain
{
    public class DomainUpdateInconsistencyException : DomainException
    {
        public DomainUpdateInconsistencyException(string message)
            : base(message)
        {
        }
    }
}
