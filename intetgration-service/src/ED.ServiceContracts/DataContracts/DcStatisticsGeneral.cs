using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcStatisticsGeneral
    {
        //TO DO - refactor - this field must be updated in the SP to NumberOfRegisteredLegalPerson
        public int NumberOfRegisteredInstitutions { get; set; }

        [DataMember]
        public int NumberOfLogins { get; set; }
        [DataMember]
        public int NumberOfRegisteredLegalPerson { get; set; }
        [DataMember]
        public int NumberOfSentMessage { get; set; }
        [DataMember]
        public int NumberOfSentMessage30days { get; set; }
        [DataMember]
        public int NumberOfSentMessage10days { get; set; }
        [DataMember]
        public int NumberOfSentMessageToday { get; set; }
        [DataMember]
        public int NumberOfRegisteredAdministrations { get; set; }
        [DataMember]
        public int NumberOfRegisteredSocialOrganisations { get; set; }
    }
}
