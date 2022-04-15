using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using EDelivery.Common.Enums;

namespace EDelivery.Common.DataContracts.ESubject
{
    [DataContract]
    public class DcElectronicIdentityInfo:IVerificationInfo
    {
        public DcElectronicIdentityInfo()
        {
            this.VerificationInfoType = eVerificationInfoType.EID;
        }
        /// <summary>
        /// Is the SAMLArtefact valid
        /// </summary>
        [DataMember]
        public bool IsValid { get; set; }
        
        [DataMember]
        public string Spin { get; set; }
        
        [DataMember]
        public string EGN { get; set; }
        
        [DataMember]
        public string PID { get; set; }
        
        [DataMember]
        public string GivenName { get; set; }

        [DataMember]
        public string GivenNameLat { get; set; }
        
        [DataMember]
        public string MiddleName { get; set; }
        
        [DataMember]
        public string MiddleNameLat { get; set; }
        
        [DataMember]
        public string FamilyName { get; set; }
        
        [DataMember]
        public string FamilyNameLat { get; set; }
        
        [DataMember]
        public DateTime? DateOfBirth { get; set; }

        [DataMember]
        public string PhoneNumber { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public eVerificationInfoType VerificationInfoType { get; set; }
    }
}
