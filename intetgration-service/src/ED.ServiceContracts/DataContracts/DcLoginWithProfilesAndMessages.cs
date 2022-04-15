using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    [KnownType(typeof(DcProfileWithMessages))]
    [KnownType(typeof(WebDcMessage))]
    [KnownType(typeof(DcProfile))]
    [KnownType(typeof(DcLogin))]
    public class DcLoginWithProfilesAndMessages
    {
        [DataMember]
        public DcLogin Login { get; set; }
        [DataMember]
        public List<DcProfileWithMessages> Profiles { get; set; }
    }

    [DataContract]
    public class DcProfileWithMessages
    {
        [DataMember]
        public DcProfile Profile { get; set; }

        [DataMember]
        public Common.Enums.eInstitutionType? InstitutionProfile { get; set; }

        [DataMember]
        public List<WebDcMessage> SentMessages { get; set; }
        [DataMember]
        public List<WebDcMessage> ReceivedMessages { get; set; }
    }
}
