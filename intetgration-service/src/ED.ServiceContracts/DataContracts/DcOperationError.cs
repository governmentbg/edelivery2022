using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcOperationError
    {
        [DataMember]
        public Common.Enums.eErrorCode ErrorCode { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }
    }
}
