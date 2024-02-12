using System;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    /// <summary>
    /// Admin information
    /// </summary>
    [DataContract]
    public class DcAdminInfo
    {
        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string MiddleName { get; set; }

        [DataMember]
        public int LoginId { get; set; }

        [DataMember]
        public string EGN { get; set; }

        [DataMember]
        public int CreatedBy { get; set; }

        [DataMember]
        public string CreatedByName { get; set; }

        [DataMember]
        public DateTime CreatedOn { get; set; }

        [DataMember]
        public DateTime? DisabledOn { get; set; }

        [DataMember]
        public string DisabledBy { get; set; }

        [DataMember]
        public string DisabledReason { get; set; }
    }

    [DataContract]
    public class DcAdminUpdateInfo : DcAdminInfo
    {
        [DataMember]
        public string PhoneNumber { get; set; }

        [DataMember]
        public string Email { get; set; }
    }
}
