using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcProfileInfo:DcProfile
    {
        [DataMember]
        public int NewMessagesCount { get; set; }
    }
}
