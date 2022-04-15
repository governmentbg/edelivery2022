using System.Runtime.Serialization;

namespace EDelivery.Common.Enums
{
    [DataContract]
    public enum eInstitutionType : int
    {
        [EnumMember]
        //Държавна администрация
        StateAdministraation = 0,
        [EnumMember]
        //Лица по чл. 1 ал 2 ЗЕУ
        SocialOrganisations = 1
    }
}
