using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.Enums
{
    [DataContract]
    public enum eErrorCode
    {
        [EnumMemberAttribute]
        SubjectAlreadyExists,

        [EnumMemberAttribute]
        InvalidDocumentFormat,

        [EnumMemberAttribute]
        InvalidSubjectIdentifier
    }
}
