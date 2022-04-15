using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegixInfoClient
{
    public static class RegixConfiguration
    {
        public static string AdministrationName { get; internal set; }
        public static string AdministrationOid { get; internal set; }
        public static string LawReason { get; internal set; }
        public static string Remark { get; internal set; }
        public static string ResponsiblePerson { get; internal set; }
        public static string ServiceType { get; internal set; }
        public static string ServiceURI { get; internal set; }

        public static void Init()
        {
            AdministrationName = ConfigurationManager.AppSettings["RegiX.AdministrationName"];
            AdministrationOid = ConfigurationManager.AppSettings["RegiX.AdministrationOid"];
            LawReason = ConfigurationManager.AppSettings["RegiX.LawReason"];
            Remark = ConfigurationManager.AppSettings["RegiX.Remark"];
            ResponsiblePerson = ConfigurationManager.AppSettings["RegiX.ResponsiblePerson"];
            ServiceType = ConfigurationManager.AppSettings["RegiX.ServiceType"];
            ServiceURI = ConfigurationManager.AppSettings["RegiX.ServiceURI"];
        }
    }
}
