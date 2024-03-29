﻿using EDelivery.Common.DataContracts.ESubject;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    [KnownType(typeof(DcSubjectPublicInfo))]
    public class DcSubjectRegistrationInfo
    {
        public DcSubjectRegistrationInfo(string identificator)
        {
            this.Identificator = identificator;
        }

        [DataMember]
        public string Identificator { get; set; }

        [DataMember]
        public bool HasRegistration { get; set; }

        [DataMember]
        public DcRegisteredSubjectInfo SubjectInfo { get; set; }
    }

    [DataContract]
    public class DcRegisteredSubjectInfo : DcSubjectPublicInfo
    {
        [DataMember]
        public Enums.eInstitutionType? InstitutionType { get; set; }
    }
}
