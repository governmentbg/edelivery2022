using System;

namespace ED.Domain
{
    internal class RnuService : IRnuService
    {
        public string Get()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
