using EDelivery.Common.Enums;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts.ESubject
{
    [DataContract]
    public class DcTokenVerificationInfo : IVerificationInfo
    {
        [DataMember]
        public bool IsValid { get; set; }
        [DataMember]
        public eVerificationInfoType VerificationInfoType { get; set; }
        [DataMember]
        public string Token { get; set; }

        public DcTokenVerificationInfo()
        {
        }
    }
}
