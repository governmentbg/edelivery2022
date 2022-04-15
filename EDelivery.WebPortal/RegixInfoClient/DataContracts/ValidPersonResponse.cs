using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegixInfoClient.DataContracts
{
    //<ValidPersonResponse xmlns="http://egov.bg/RegiX/GRAO/NBD/ValidPersonResponse"><FirstName>НАТАЛИЯ</FirstName><c>ЛЪЧЕЗАРОВА</SurName><FamilyName>ЛАЗАРОВА</FamilyName><BirthDate>1985-08-03</BirthDate></ValidPersonResponse>
    public class ValidPersonResponse
    {
        public string ErrorMessage { get; set; }
        public bool Success { get; set; }
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public string FamilyName { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
