using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.SEOS.DatabaseAccess
{
    public enum TransferTypeEnum : short
    {
        FromAS4toAS4 = 0,
        FromAS4toSEOS = 1,
        FromSEOStoAS4 = 2
    }
}
