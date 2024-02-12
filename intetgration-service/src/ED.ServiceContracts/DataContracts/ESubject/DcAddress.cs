using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts.ESubject
{
    [DataContract]
    public class DcAddress
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string CountryIso2 { get; set; }

        [DataMember]
        public string State { get; set; }

        [DataMember]
        public string City { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public string ZipCode { get; set; }

        public string GetFullAddress(Dictionary<string, string> countries)
        {
            if (string.IsNullOrEmpty(this.CountryIso2))
            {
                return this.Address;
            }

            var country = countries.FirstOrDefault(x => x.Key == this.CountryIso2).Value;
            return $"{country}, {(!String.IsNullOrWhiteSpace(this.State) ? this.State + ", " : "")}{this.City}, {this.Address}";
        }
    }
}
