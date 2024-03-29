﻿using System;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcProfileHistoryRecordDetails
    {
        [DataMember]
        public string RelatedUserName { get; set; }

        [DataMember]
        public Guid RelatedUserGuid { get; set; }

        [DataMember]
        public string RelatedData { get; set; }
    }
}
