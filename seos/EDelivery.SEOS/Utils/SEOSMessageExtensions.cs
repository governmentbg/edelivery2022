using EDelivery.SEOS.DataContracts;
using log4net;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace EDelivery.SEOS.Utils
{
    public static class SEOSMessageExtensions
    {
        private static ILog logger = LogManager.GetLogger("SEOSXmlParser");
        public static bool LoadIfValidXmlDocument(this string message, out XmlDocument doc)
        {
            doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            
            try
            {
                doc.LoadXml(message);
                return doc != null && doc.DocumentElement != null;
            }
            catch (Exception ex)
            {
                //log error
                logger.Error($"Error in loading xml", ex);
                return false;
            }
        }

        public static XmlDocument SerializeMessage(this Message message)
        {
            try
            {
                string str;
                using (var stw = new Utf8StringWriter())
                {
                    using (var xw = XmlWriter.Create(stw, new XmlWriterSettings() { Encoding = System.Text.Encoding.UTF8 }))
                    {
                        var serializer = new XmlSerializer(typeof(Message), "http://schemas.egov.bg/messaging/v1");
                        serializer.Serialize(xw, message);

                    }
                    str = stw.ToString();
                }
                var xml = new XmlDocument();
                xml.LoadXml(str);
                return xml;
            }
            catch (Exception ex)
            {
                //log error
                logger.Error($"Error in serializing message obj to xml", ex);
                return null;
            }
        }

        public static Message DeserializeToMessage(this string message)
        {
            try
            {
                XmlSerializer des = new XmlSerializer(typeof(Message));
                var resultObj = (Message)des.Deserialize(new StringReader(message));
                return resultObj;
            }
            catch (Exception ex)
            {
                //log error
                logger.Error($"Error in parsing string {message} to Message object", ex);
                return null;
            }
        }

        public static Guid GetMessageGuid(this Message message)
        {
            var guid = message.Header.MessageGUID;
            if (String.IsNullOrEmpty(guid))
                return Guid.Empty;

            return Guid.Parse(guid);
        }

        public static Guid GetDocumentGuid(this Message message)
        {
            var identity = message.GetDocIdentity();
            if (identity == null || String.IsNullOrEmpty(identity.DocumentGUID))
                return Guid.Empty;

            return Guid.Parse(identity.DocumentGUID);
        }

        public static DocumentIdentificationType GetDocIdentity(this Message message)
        {
            var item = message.Body.Item;

            if (item is DocumentRegistrationRequestType)
            {
                return (item as DocumentRegistrationRequestType).Document?.DocID;
            }

            if (item is DocumentStatusRequestType)
            {
                return (item as DocumentStatusRequestType).DocID;
            }

            if (item is DocumentStatusResponseType)
            {
                return (item as DocumentStatusResponseType).DocID;
            }

            return null;
        }

        public static Tuple<string, DateTime?> GetDocNumberAndDate(this DocumentIdentificationType identity, DateTime? defaultDate)
        {
            string docNumber = String.Empty;
            DateTime? docDate = null;

            if (identity.Item is DocumentURI)
            {
                var docUri = (identity.Item as DocumentURI);
                docNumber = $"{docUri.RegisterIndex}-{ docUri.SequenceNumber}";
                docDate = docUri.ReceiptOrSigningDateSpecified
                    ? docUri.ReceiptOrSigningDate
                    : defaultDate;
            }

            if(identity.Item is DocumentNumberType)
            {
                var docNumberT = (identity.Item as DocumentNumberType);
                docNumber = docNumberT.DocNumber;
                docDate = docNumberT.DocDate;
            }

            return new Tuple<string, DateTime?>(docNumber, docDate);
        }

        public static Tuple<string, DateTime?> GetDocNumberAndDate(this Message message)
        {
            var identity = message.GetDocIdentity();
            if (identity == null)
                return new Tuple<string, DateTime?>(String.Empty, null);

            return identity.GetDocNumberAndDate(message.Header.MessageDate);
        }

        public static Tuple<string, DateTime?> GetParDocNumberAndDate(this Message message)
        {
            var item = message.Body.Item;
            if (!(item is DocumentRegistrationRequestType))
                return new Tuple<string, DateTime?>(String.Empty, null);

            var parIdentity = (item as DocumentRegistrationRequestType)
                .Document?.DocParentID;
            if (parIdentity == null || parIdentity.Item == null)
                return new Tuple<string, DateTime?>(String.Empty, null);

            return parIdentity.GetDocNumberAndDate(message.Header.MessageDate);
        }

        public static Guid? GetParDocumentGuid(this Message message)
        {
            var item = message.Body.Item;
            if (!(item is DocumentRegistrationRequestType))
                return null;

            var parIdentity = (item as DocumentRegistrationRequestType)
                .Document?.DocParentID;
            if (parIdentity == null || String.IsNullOrEmpty(parIdentity.DocumentGUID))
                return null;

            return Guid.Parse(parIdentity.DocumentGUID);
        }
    }
}
