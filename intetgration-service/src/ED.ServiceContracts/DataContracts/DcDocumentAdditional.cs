﻿using System;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcDocumentAdditional
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string DocumentDescription { get; set; }

        [DataMember]
        public string DocumentName { get; set; }

        [DataMember]
        public int? ContentEncodingCodePage { get; set; }

        [DataMember]
        public string ContentType { get; set; }

        [DataMember]
        public byte[] Content { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        //[DataMember]
        //public List<DcSignatureValidationResult> SignaturesInfo { get; set; }
    }

}
