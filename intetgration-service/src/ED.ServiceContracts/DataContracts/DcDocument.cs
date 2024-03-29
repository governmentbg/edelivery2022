﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcDocument
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string DocumentName { get; set; }

        [DataMember]
        public int? ContentEncodingCodePage { get; set; }

        [DataMember]
        public string ContentType { get; set; }

        [DataMember]
        public byte[] Content { get; set; }

        [DataMember]
        public DcTimeStamp TimeStamp;

        [DataMember]
        public string DocumentRegistrationNumber { get; set; }

        [DataMember]
        public List<DcSignatureValidationResult> SignaturesInfo { get; set; }
    }

}
