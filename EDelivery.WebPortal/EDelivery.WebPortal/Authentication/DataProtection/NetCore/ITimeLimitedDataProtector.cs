using Microsoft.Owin.Security.DataProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDelivery.WebPortal
{
    public interface ITimeLimitedDataProtector : IDataProtector
    {
        byte[] Protect(byte[] plaintext, DateTimeOffset expiration);
        byte[] Unprotect(byte[] protectedData, out DateTimeOffset expiration);
    }
}
