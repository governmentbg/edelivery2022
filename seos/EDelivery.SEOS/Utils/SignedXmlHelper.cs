using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using EDelivery.SEOS.DataContracts;

namespace EDelivery.SEOS.Utils
{
    public static class SignedXmlHelper
    {
        /// <summary>
        /// Sign xml document with the provided signature
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public static XmlDocument SignXML(XmlDocument xmlDocument, X509Certificate2 certificate)
        {
            var signedXml = new SignedXml(xmlDocument);

            var rsaCsp = (RSACryptoServiceProvider)certificate.PrivateKey;
            signedXml.SigningKey = rsaCsp;

            // Create a reference to be signed.
            var reference = new Reference();
            reference.Uri = "";

            // Add an enveloped transformation to the reference.
            var env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            // Add the reference to the SignedXml object.
            signedXml.AddReference(reference);

            // Add an RSAKeyValue KeyInfo (optional; helps recipient find key to validate).
            var keyInfo = new KeyInfo();

            var xserial = new X509IssuerSerial();
            xserial.IssuerName = certificate.IssuerName.Name.ToString();
            xserial.SerialNumber = certificate.SerialNumber;

            var keyInfoData = new KeyInfoX509Data(certificate, X509IncludeOption.EndCertOnly);
            keyInfoData.AddCertificate(certificate);
            keyInfoData.AddIssuerSerial(xserial.IssuerName, xserial.SerialNumber);
            keyInfo.AddClause(keyInfoData);
            signedXml.KeyInfo = keyInfo;

            // Compute the signature.
            signedXml.ComputeSignature();

            // Get the XML representation of the signature and save
            // it to an XmlElement object.
            var xmlDigitalSignature = signedXml.GetXml();

            xmlDocument.DocumentElement.AppendChild(xmlDocument.ImportNode(xmlDigitalSignature, true));
            return xmlDocument;
        }



        /// <summary>
        /// Validate xml signature
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <param name="certSerialNumber"></param>
        /// <returns></returns>
        public static bool ValidateXmlSignature(XmlDocument xmlDocument)
        {
            string certSerialNumber;
            return ValidateXmlSignature(xmlDocument, out certSerialNumber);
        }

        /// <summary>
        /// Validate xml signature and get the signature certificate
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <param name="certSerialNumber"></param>
        /// <returns></returns>
        public static bool ValidateXmlSignature(XmlDocument xmlDocument, out string certSerialNumber)
        {
            certSerialNumber = "";

            var signedXml = new SignedXml(xmlDocument);

            var nodeSignatureList = xmlDocument.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#");

            if (nodeSignatureList.Count == 0)
                return false;

            var nodeCertList = xmlDocument.GetElementsByTagName("X509Certificate", "http://www.w3.org/2000/09/xmldsig#");
            if (nodeCertList.Count == 0)
                return false;

            signedXml.LoadXml((XmlElement)nodeSignatureList[0]);

            using (var certificate = new X509Certificate2())
            {
                var strCertificateBase64 = nodeCertList[0].InnerText;
                certificate.Import(Convert.FromBase64String(strCertificateBase64));

                var alg = (RSACryptoServiceProvider)certificate.PublicKey.Key;
                var check = signedXml.CheckSignature(alg);
                certSerialNumber = certificate.SerialNumber;

                return check;
            }
        }

        /// <summary>
        /// Add message headers and sign the message
        /// </summary>
        /// <param name="sender">message sender</param>
        /// <param name="receiver">message receiver</param>
        /// <param name="messageType">message type</param>
        /// <param name="body">message body</param>
        /// <returns>Signed message, ready to be sent</returns>
        public static string WrapAndSignMessage(EntityNodeType sender, EntityNodeType receiver, MessageType messageType, object body, Guid? messageGuid = null)
        {
            //add header
            var mesageGuid = messageGuid ?? Guid.NewGuid();
            var message = new Message()
            {
                Header = new MessageHeader()
                {
                    MessageDate = DateTime.Now,
                    MessageGUID = mesageGuid.ToString("B"),
                    MessageType = messageType,
                    Recipient = receiver,
                    Sender = sender,
                    Version = "1.0",
                },
                Body = new MessageBody()
                {
                    Item = body
                }
            };

            //create xml
            XmlDocument messageXml = message.SerializeMessage();
            var signerXml = SignedXmlHelper.SignXML(messageXml, CertificateHelper.SEOSCertificate);
            if (!SignedXmlHelper.ValidateXmlSignature(signerXml))
            {
                return null;
            }
            return signerXml.OuterXml;
        }

        public static string SignMessageWithEDeliveryCertificate(Message message)
        {
            var messageRedirect = new Message()
            {
                Header = message.Header,
                Body = message.Body
            };

            XmlDocument messageXml = messageRedirect.SerializeMessage();
            var signerXml = SignedXmlHelper.SignXML(messageXml, CertificateHelper.SEOSCertificate);
            if (!SignedXmlHelper.ValidateXmlSignature(signerXml))
            {
                return null;
            }
            return signerXml.OuterXml;
        }
    }
}
