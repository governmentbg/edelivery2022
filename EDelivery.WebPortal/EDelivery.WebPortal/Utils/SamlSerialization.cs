using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace EDelivery.WebPortal.Utils
{
    public static class SamlSerialization
    {
        static SamlSerialization()
        {
            XmlNamespaces = new XmlSerializerNamespaces();
            XmlNamespaces.Add("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
            XmlNamespaces.Add("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
        }
        
        public static T Deserialize<T>(XmlReader reader)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return (T) serializer.Deserialize(reader);
        }
        
        public static T DeserializeFromXmlString<T>(string xml)
        {
            XmlTextReader reader = new XmlTextReader(new StringReader(xml));
            return Deserialize<T>(reader);
        }
        
        public static XmlDocument Serialize<T>(T item, XmlSerializerNamespaces namespaces=null)
        {
            MemoryStream stream = new MemoryStream();
            Serialize<T>(item, stream, namespaces);
            XmlDocument document = new XmlDocument();
            stream.Seek(0L, SeekOrigin.Begin);
            document.Load(stream);
            stream.Close();
            return document;
        }

        public static void Serialize<T>(T item, Stream stream, XmlSerializerNamespaces namespaces = null)
        {
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(stream, item, namespaces??XmlNamespaces);
            stream.Flush();
        }
        
        public static string SerializeToXmlString<T>(T item)
        {
            MemoryStream stream = new MemoryStream();
            Serialize<T>(item, stream);
            StreamReader reader = new StreamReader(stream);
            stream.Seek(0L, SeekOrigin.Begin);
            return reader.ReadToEnd();
        }

        public static XmlSerializerNamespaces XmlNamespaces { get; set; }
    }
}