using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDelivery.WebPortal.Utils.DbAuditLogs
{
    public enum JWTTokenType : int
    {
        Ahu = 1,
        Payments = 2,
        Regex = 3,
        BulSI = 4,
        Test = 5
    }
}