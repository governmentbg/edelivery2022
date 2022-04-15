using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using log4net;

namespace EDelivery.SEOS.Utils
{
    public class XsdValidationHelper
    {
        private string Message { get; set; }

        private ILog Logger { get; set; }

        public bool IsValid { get; protected set; }

        public string Error { get; protected set; }

        public XsdValidationHelper(
            string message, 
            ILog logger)
        {
            this.Message = message;
            this.Logger = logger;
            this.IsValid = true;
        }

        public (bool IsValid, string Error) Validate()
        {
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            Add(
                schemaSet, 
                "http://schemas.egov.bg/messaging/v1", 
                "EGovMessaging.xsd");
            Add(
                schemaSet, 
                "http://www.w3.org/2000/09/xmldsig#", 
                "xmldsig-core-schema.xsd");
            Add(
                schemaSet, 
                "http://ereg.egov.bg/segment/0009-000001", 
                "0009-000001_DocumentURI.xsd");
            Add(
                schemaSet, 
                "http://ereg.egov.bg/value/0008-000002", 
                "0008-000002_RegisterIndex.xsd");
            Add(
                schemaSet, 
                "http://ereg.egov.bg/value/0008-000003", 
                "0008-000003_DocumentSequenceNumber.xsd");
            Add(
                schemaSet, 
                "http://ereg.egov.bg/value/0008-000004", 
                "0008-000004_DocumentReceiptOrSigningDate.xsd");
            Add(
                schemaSet, 
                "http://ereg.egov.bg/value/0008-000001", 
                "0008-000001_BatchNumber.xsd");

            var doc = new XmlDocument();
            doc.LoadXml(this.Message);
            doc.Schemas.Add(schemaSet);
            doc.Validate(Validation);

            return (this.IsValid, this.Error);
        }

        private void Validation(
            object sender, 
            ValidationEventArgs e)
        {
            this.IsValid = false;
            this.Error = e.Message;
            this.Logger.Error(e.Exception);
        }

        private static void Add(
            XmlSchemaSet set, 
            string xsdNamespace, 
            string xsdFileName)
        {
            var resPath = $"EDelivery.SEOS.Resources.Xsd.{xsdFileName}";
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(resPath))
            using (var reader = new StreamReader(stream))
            using (var xmlReader = new XmlTextReader(reader))
            {
                set.Add(xsdNamespace, xmlReader);
            }
        }
    }
}
