using System.Security.Cryptography.X509Certificates;

namespace ED.Domain
{
    public class RegixOptions
    {
        public RegixOptionsSettings Settings { get; set; } = null!;

        public RegixOptionsCertificate Certificate { get; set; } = null!;
    }

    public class RegixOptionsSettings
    {
        public string AdministrationName { get; set; } = null!;

        public string AdministrationOid { get; set; } = null!;

        public string LawReason { get; set; } = null!;

        public string Remark { get; set; } = null!;

        public string ResponsiblePerson { get; set; } = null!;

        public string ServiceType { get; set; } = null!;

        public string ServiceURI { get; set; } = null!;

        public string Endpoint { get; set; } = null!;
    }

    public class RegixOptionsCertificate
    {
        public string? Store { get; set; }

        public StoreLocation? StoreLocation { get; set; }

        public string? Name { get; set; }
    }
}
