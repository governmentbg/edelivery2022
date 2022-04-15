using EDelivery.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts.ESubject
{
    [DataContract]
    public class DcAdministrativeActInfo: IVerificationInfo
    {
        public DcAdministrativeActInfo()
        {
            this.VerificationInfoType = eVerificationInfoType.AdministrativeAct;
        }

        [DataMember]
        public string ActNumber { get; set; }

        [DataMember]
        public string CreatedByInstitution { get; set; }

        [DataMember]
        public DateTime CreatedOnDate { get; set; }

        [DataMember]
        public bool IsValid { get; set; }

        [DataMember]
        public eVerificationInfoType VerificationInfoType { get; set; }
    }
}
