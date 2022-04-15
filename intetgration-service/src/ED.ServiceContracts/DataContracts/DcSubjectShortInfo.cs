using EDelivery.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    /// <summary>
    /// Short info for subject
    /// </summary>
    [DataContract]
    public class DcSubjectShortInfo
    {
        [DataMember]
        public eProfileType ProfileType { get; set; }

        [DataMember]
        public string EIK { get; set; }

        [DataMember]
        public string EGN { get; set; }

        [DataMember]
        public string Name { get; set; }
    }

}
