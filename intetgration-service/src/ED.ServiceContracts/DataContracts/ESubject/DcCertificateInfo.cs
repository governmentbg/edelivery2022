using EDelivery.Common.Enums;
using System;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts.ESubject
{
    public interface IVerificationInfo
    {
        bool IsValid { get; set; }

        eVerificationInfoType VerificationInfoType { get; set; }
    }

    [DataContract]
    public class DcCertificateInfo : IVerificationInfo
    {
        public DcCertificateInfo()
        {
            this.VerificationInfoType = eVerificationInfoType.Certificate;
        }

        [DataMember]
        public string Issuer { get; set; }

        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public DateTime ValidFrom { get; set; }

        [DataMember]
        public DateTime ValidTo { get; set; }

        [DataMember]
        public bool IsValid { get; set; }

        [DataMember]
        public eVerificationInfoType VerificationInfoType { get; set; }
    }
}
