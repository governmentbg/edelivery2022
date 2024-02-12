using System;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcInstitutionListItem
    {
        [DataMember]
        public Guid ElectronicSubjectId { get; set; }

        [DataMember]
        public string UniqueIdentifier { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string HeadInstitutionName { get; set; }

        [DataMember]
        public Guid? HeadInstitutionId { get; set; }

        [DataMember]
        public bool IsReadOnly { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public int TypeId { get; set; }

        [DataMember]
        public DateTime CreatedOn { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }
    }
}
