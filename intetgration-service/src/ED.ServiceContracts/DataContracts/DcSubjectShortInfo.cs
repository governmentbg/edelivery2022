using EDelivery.Common.Enums;
using System.Runtime.Serialization;

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
