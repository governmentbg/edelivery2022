using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [Serializable]
    public class DcSignatureValidationResult
    {
        [DataMember]
        public Enums.eVerificationResult Status { get; set; }

        [DataMember]
        public Enums.eRevokationResult RevocationStatus { get; set; }

        [DataMember]
        public List<string> ChainErrors { get; set; }

        [DataMember]
        public List<DcChainCertificate> ChainCertificates { get; set; }

        [DataMember]
        public string CertificateAlgorithm { get; set; }

        [DataMember]
        public DateTime ValidTo { get; set; }

        [DataMember]
        public DateTime ValidFrom { get; set; }

        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public string SubjectEGN { get; set; }

        [DataMember]
        public string Issuer { get; set; }

        [DataMember]
        public bool IsExpired { get; set; }

        [DataMember]
        public bool IsTrustedSigner { get; set; }

        [DataMember]
        public bool IsSignatureValid { get; set; }

        [DataMember]
        public bool IsIntegrityValid { get; set; }

        [DataMember]
        public string Format { get; set; }

        [DataMember]
        public bool ContainsTimeStamp { get; set; }

        [DataMember]
        public string TimeStampAuthority { get; set; }

        [DataMember]
        public DateTime? TimeStampDate { get; set; }
    }
}
