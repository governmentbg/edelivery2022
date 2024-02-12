using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcLogin
    {
        [DataMember]
        public int Id { get; set; }


        [DataMember]
        public Guid ElectronicSubjectId { get; set; }

        [DataMember]
        public string ElectronicSubjectName { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string PhoneNumber { get; set; }

        [DataMember]
        public List<DcProfile> Profiles { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public string CertificateThumbprint { get; set; }
        [DataMember]
        public string PushNotificationsUrl { get; set; }
        public DcProfile DefaultProfile
        {
            get
            {
                return this.Profiles.Any() ? this.Profiles.SingleOrDefault(x => x.IsDefault) : null;
            }
        }

    }
}
