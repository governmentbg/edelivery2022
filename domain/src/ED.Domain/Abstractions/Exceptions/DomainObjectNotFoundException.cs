using System;
using System.Linq;

namespace ED.Domain
{
    public class DomainObjectNotFoundException : Exception
    {
        public DomainObjectNotFoundException(string entitySet)
            : base($"Cannot find entity from set {entitySet}")
        {
        }

        public DomainObjectNotFoundException(string entitySet, object[] keyValues)
            : base($"Cannot find entity from set {entitySet} with ids {string.Join(",", keyValues.Select(k => k.ToString()).ToArray())}")
        {
        }
    }
}
