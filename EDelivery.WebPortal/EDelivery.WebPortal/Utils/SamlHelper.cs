using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Serialization;

using EDelivery.WebPortal.Enums;
using EDelivery.WebPortal.Models;

using log4net;

using SAML2;
using SAML2.Config;
using SAML2.Schema.Protocol;
using SAML2.Utils;


namespace EDelivery.WebPortal.Utils
{
    public class SamlHelper
    {
        private static ILog logger = LogManager.GetLogger("SamlHelper");
        public static class SamlConfiguration
        {
            public static int SamlVersion { get; }
            public static string LoginUrl { get; }
            public static string ReturnUrl { get; }
            public static string TargetUrl { get; }
            public static string ProviderName { get; }
            public static string ProviderId { get; }
            public static string ExtServiceId { get; }
            public static string ExtProviderId { get; }

            static SamlConfiguration()
            {
                ProviderId = WebConfigurationManager.AppSettings["bg.gov.eAuth.providerId"];
                ExtServiceId = WebConfigurationManager.AppSettings["bg.gov.eAuth.extService"]; ;
                ExtProviderId = WebConfigurationManager.AppSettings["bg.gov.eAuth.extProvider"]; ;

                SamlVersion = Int32.Parse(WebConfigurationManager.AppSettings["bg.gov.eAuth.version"] ?? "1");
                LoginUrl = WebConfigurationManager.AppSettings[$"bg.gov.eAuth.{SamlVersion}.loginUrl".Replace("..", ".")];
                ReturnUrl = WebConfigurationManager.AppSettings[$"bg.gov.eAuth.{SamlVersion}.returnUrl".Replace("..", ".")];
                TargetUrl = WebConfigurationManager.AppSettings[$"bg.gov.eAuth.{SamlVersion}.targetUrl".Replace("..", ".")];
                ProviderName = WebConfigurationManager.AppSettings[$"bg.gov.eAuth.{SamlVersion}.providerName".Replace("..", ".")];
            }

        }

        #region Old

        ///// <summary>
        ///// Generate Saml artifact resolve request and return it wrapped in SOAP envelope
        ///// </summary>
        ///// <param name="artifactStr">the artifact</param>
        ///// <returns>soap envelope string</returns>
        //public static string GenerateSaml2ArtifactResolve(string artifactStr)
        //{
        //    var resolve = new SAML2.Saml20ArtifactResolve();
        //    resolve.Artifact = artifactStr;

        //    //get the element
        //    var samlElement = resolve.Resolve;
        //    //get xml document
        //    var xml = GetXmlDocumentFromSaml(samlElement);
        //    var artifactResolveString = xml.DocumentElement.OuterXml;
        //    //return soap packed message
        //    return WrapInSoapMessage(artifactResolveString);
        //}


        ///// <summary>
        ///// Parse the result and return eID user data
        ///// </summary>
        ///// <param name="result"></param>
        ///// <returns></returns>
        //public static DcElectronicIdentityInfo ParseSaml2Result(string result, out Status status)
        //{
        //    var soapdoc = new XmlDocument();
        //    soapdoc.LoadXml(result);
        //    var soapNS = new XmlNamespaceManager(soapdoc.NameTable);
        //    soapNS.AddNamespace("soapenv", soapdoc.DocumentElement.GetNamespaceOfPrefix("soapenv"));

        //    var samlElement = soapdoc.DocumentElement.SelectSingleNode("//soapenv:Body", soapNS).FirstChild;
        //    var samlNS = new XmlNamespaceManager(soapdoc.NameTable);
        //    samlNS.AddNamespace("pr", "urn:person:eid.egov.bg");
        //    samlNS.AddNamespace("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");
        //    samlNS.AddNamespace("saml2p", "urn:oasis:names:tc:SAML:2.0:protocol");
        //    var statusCode = samlElement.SelectSingleNode("//saml2p:Status/saml2p:StatusCode/@Value", samlNS).Value;
        //    var statusMessage = HttpUtility.HtmlDecode(samlElement.SelectSingleNode("//saml2p:Status/saml2p:StatusMessage", samlNS).InnerText);
        //    status = new Status() { StatusCode = new StatusCode() { Value = statusCode }, StatusMessage = statusMessage };

        //    if (statusCode == SAML2.Saml20Constants.StatusCodes.Success)
        //    {
        //        var personData = new DcElectronicIdentityInfo();
        //        personData.IsValid = true;
        //        var personNode = samlElement.SelectSingleNode("//pr:Person", samlNS);
        //        if (personNode == null) return null;

        //        personData.EGN = personNode.SelectSingleNode("//pr:Egn", samlNS).InnerText;
        //        personData.GivenName = personNode.SelectSingleNode("//pr:Name/pr:GivenName", samlNS).InnerText;
        //        personData.MiddleName = personNode.SelectSingleNode("//pr:Name/pr:MiddleName", samlNS).InnerText;
        //        personData.FamilyName = personNode.SelectSingleNode("//pr:Name/pr:FamilyName", samlNS).InnerText;
        //        personData.GivenNameLat = personNode.SelectSingleNode("//pr:NameLat/pr:GivenName", samlNS).InnerText;
        //        personData.MiddleNameLat = personNode.SelectSingleNode("//pr:NameLat/pr:MiddleName", samlNS).InnerText;
        //        personData.FamilyNameLat = personNode.SelectSingleNode("//pr:NameLat/pr:FamilyName", samlNS).InnerText;
        //        personData.Spin = personNode.SelectSingleNode("//pr:Spin", samlNS).InnerText;
        //        var birthDate = personNode.SelectSingleNode("//pr:DateOfBirth", samlNS);
        //        DateTime date;
        //        if (birthDate != null && DateTime.TryParse(birthDate.InnerText, out date))
        //        {
        //            personData.DateOfBirth = date;
        //        }
        //        else
        //        {
        //            personData.DateOfBirth = TextHelper.GetBirthDateFromEGN(personData.EGN);
        //        }

        //        var phoneElement = samlElement.SelectNodes("//saml2:Attribute[@Name='phoneNumber']", samlNS);
        //        if (phoneElement != null && phoneElement.Count > 0)
        //        {
        //            personData.PhoneNumber = phoneElement[0].FirstChild.InnerText;
        //        }
        //        var addressElement = samlElement.SelectNodes("//saml2:Attribute[@Name='address']", samlNS);
        //        if (addressElement != null && addressElement.Count > 0)
        //        {
        //            personData.Address = addressElement[0].FirstChild.InnerText;
        //        }

        //        return personData;
        //    }
        //    return null;
        //}

        ///// <summary>
        ///// Serialize saml2 object to xml
        ///// </summary>
        ///// <param name="samlObj">Saml2 object</param>
        ///// <returns></returns>
        //public static XmlDocument GetXmlDocumentFromSaml<T>(T samlObj)
        //{
        //    XmlDocument document = new XmlDocument
        //    {
        //        PreserveWhitespace = true
        //    };
        //    document.LoadXml(SamlSerialization.SerializeToXmlString<T>(samlObj));
        //    return document;
        //}

        ///// <summary>
        ///// Wrap the given string in a soap message
        ///// </summary>
        ///// <param name="body"></param>
        ///// <returns></returns>
        //public static string WrapInSoapMessage(string body)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    builder.AppendLine("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\">");
        //    builder.AppendLine("<SOAP-ENV:Body>");
        //    builder.AppendLine(body);
        //    builder.AppendLine("</SOAP-ENV:Body>");
        //    builder.AppendLine("</SOAP-ENV:Envelope>");
        //    return builder.ToString();
        //}

        ///// <summary>
        ///// Send web request to obtain soap response
        ///// </summary>
        ///// <param name="message"></param>
        ///// <param name="URL"></param>
        ///// <returns></returns>
        //public static string SendWebRequest(string message, string URL)
        //{
        //    using (WebClient client = new WebClient())
        //    {
        //        client.Headers.Add(HttpRequestHeader.ContentType, "application/soap+xml; charset=utf-8");
        //        client.Headers.Add("SOAPAction", URL);
        //        var result = client.UploadString(URL, message);
        //        return result;
        //    }
        //}
        #endregion Old

        #region SAML10

        private static object loadCertLock = new object();
        /// <summary>
        /// eID success status
        /// </summary>
        internal static readonly string eIDSuccessStatus = "urn:oasis:names:tc:SAML:2.0:status:Success";



        /// <summary>
        /// Load the certificate
        /// </summary>
        /// <param name="pathToCertificate"></param>
        /// <returns></returns>
        internal static X509Certificate2 LoadCertificate(string pathToCertificate, string certPassword)
        {

            if (string.IsNullOrEmpty(pathToCertificate))
            {
                throw new ArgumentNullException("pathToCertificate");
            }

            if (!System.IO.Path.IsPathRooted(pathToCertificate))
            {
                var dirname = HttpContext.Current.Server.MapPath("~/");
                pathToCertificate = System.IO.Path.Combine(dirname, pathToCertificate);
            }
            if (!File.Exists(pathToCertificate))
            {
                ElmahLogger.Instance.Error($"Certificate for signing AuthNRequest does not existst: Path: {pathToCertificate}!");
                throw new ArgumentOutOfRangeException("pathToCertificate");
            }

            System.Security.Cryptography.X509Certificates.X509Certificate2 cert;
            try
            {
                lock (loadCertLock)
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(pathToCertificate);
                    cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(bytes, certPassword);
                    return cert;
                }
            }
            catch (FileNotFoundException fnfException)
            {
                ElmahLogger.Instance.Error(fnfException, $"Can not load Certificate from path {pathToCertificate}. FileNotFoundException exception!");
                if (!pathToCertificate.Contains("copy"))
                {
                    var ext = Path.GetExtension(pathToCertificate);
                    pathToCertificate = $"{pathToCertificate.Replace(ext, string.Empty)}-copy{ext}";
                    return LoadCertificate(pathToCertificate, certPassword);
                }
                return null;

            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(ex, $"Can not load Site Certificate from path {pathToCertificate} or the password {certPassword} is Invalid! Please check!");
                return null;
            }
        }

        /// <summary>
        /// Sign an xml document with the provided certificate
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <param name="pathToCertificate"></param>
        /// <returns></returns>
        internal static string SignXmlDocument(XmlDocument xmlDocument, X509Certificate2 certificate)
        {
            //get the reference to be signed
            string reference = xmlDocument.DocumentElement.GetAttribute("ID");

            var signedDocument = SignDocument(xmlDocument, certificate, "");
            var signedDocumentString = SignedDocumentToString(signedDocument);
            bool verified = VerifyXmlFile(signedDocumentString, "Signature");
            if (!verified)
            {
                ElmahLogger.Instance.Error("Can not verify signed AuthNRequest xml");
            }
            return signedDocumentString;
        }

        /// <summary>
        /// Sign an xml document
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="cert"></param>
        /// <param name="referenceId"></param>
        /// <returns></returns>
        private static XmlDocument SignDocument(XmlDocument doc, X509Certificate2 cert, string referenceId)
        {
            SignedXml xml = new SignedXml(doc)
            {
                SignedInfo =  {
                                CanonicalizationMethod = SignedXml.XmlDsigExcC14NWithCommentsTransformUrl,
                                SignatureMethod = SignedXml.XmlDsigRSASHA1Url
                            },
                SigningKey = cert.PrivateKey,

            };

            Reference reference = new Reference(referenceId);
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigExcC14NTransform());
            xml.AddReference(reference);

            xml.KeyInfo = new KeyInfo();
            var keyInfoData = new KeyInfoX509Data(cert, X509IncludeOption.EndCertOnly);
            keyInfoData.AddIssuerSerial(cert.Issuer, cert.SerialNumber);
            keyInfoData.AddSubjectName(cert.Subject);
            xml.KeyInfo.AddClause(keyInfoData);
            xml.ComputeSignature();

            var signatureXml = xml.GetXml();

            //signatureXml
            if (doc.DocumentElement != null)
            {
                XmlNodeList elementsByTagName = doc.DocumentElement.GetElementsByTagName("Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
                System.Xml.XmlNode parentNode = elementsByTagName[0].ParentNode;
                if (parentNode != null)
                {
                    parentNode.InsertAfter(doc.ImportNode(signatureXml, true), elementsByTagName[0]);
                }
            }
            return doc;
        }

        /// <summary>
        /// Writes a document to a string
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static string SignedDocumentToString(XmlDocument doc)
        {
            // Save the signed XML document to a file specified 
            // using the passed string.
            StringBuilder sb = new StringBuilder();
            using (TextWriter sw = new StringWriter(sb))
            {
                using (XmlTextWriter xmltw = new XmlTextWriter(sw))
                {
                    doc.WriteTo(xmltw);
                }
            }
            return sb.ToString();

        }

        public static Boolean VerifyXmlFile(String xmlString, string signatureTag)
        {
            // Create a new XML document.
            XmlDocument xmlDocument = new XmlDocument();

            // Format using white spaces.
            //xmlDocument.PreserveWhitespace = true;

            // Load the passed XML file into the document. 
            xmlDocument.LoadXml(xmlString);

            // Create a new SignedXml object and pass it 
            // the XML document class.
            SignedXml signedXml = new SignedXml(xmlDocument.DocumentElement);

            // Find the "Signature" node and create a new 
            // XmlNodeList object.
            XmlNodeList nodeList = xmlDocument.GetElementsByTagName(signatureTag);

            // Load the signature node.
            signedXml.LoadXml((XmlElement)nodeList[0]);

            // Check the signature and return the result. 
            return signedXml.CheckSignature();

        }


        private static void SetPrefix(string prefix, XmlNode node)
        {
            foreach (XmlNode n in node.ChildNodes)
                SetPrefix(prefix, n);
            node.Prefix = prefix;
        }

        /// <summary>
        /// Parce cert samlp:Response object
        /// </summary>
        /// <param name="SamlResponse"></param>
        /// <returns></returns>
        internal static CertificateAuthResponse ParseSaml2CertificateResult(string SamlResponse)
        {
            var certAuthResponce = new CertificateAuthResponse();
            ElmahLogger.Instance.Error("eAuth saml response: " + SamlResponse);
            if (string.IsNullOrEmpty(SamlResponse))
            {
                throw new ArgumentNullException("SamlResponse");
            }
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(SamlResponse);
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(ex, "Can not load samlresponse object!");
                certAuthResponce.ResponseStatus = Enums.eCertResponseStatus.InvalidResponseXML;
                return certAuthResponce;
            }

            bool valid = VerifyXmlFile(SamlResponse, "ds:Signature");
            if (!valid)
            {

                ElmahLogger.Instance.Error("SamlResponse Invalid Signature!");
                certAuthResponce.ResponseStatus = Enums.eCertResponseStatus.InvalidSignature;
                return certAuthResponce;
            }

            try
            {
                var responseElement = doc.DocumentElement;

                var samlNS = new XmlNamespaceManager(doc.NameTable);
                samlNS.AddNamespace("egovbga", "urn:bg:egov:eauth:1.0:saml:ext");
                samlNS.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
                samlNS.AddNamespace("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");
                samlNS.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
                samlNS.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
                var statusCode = responseElement.SelectSingleNode("//samlp:Status/samlp:StatusCode", samlNS);
                if (statusCode != null)
                {
                    var statusCodeValue = statusCode.Attributes["Value"].Value;
                    var innerStatusCode = statusCode.SelectSingleNode("samlp:StatusCode", samlNS);
                    if (innerStatusCode != null)
                    {
                        statusCodeValue = innerStatusCode.Attributes["Value"].Value;
                    }
                    var statusMessage = responseElement.SelectSingleNode("//samlp:Status/samlp:StatusMessage", samlNS);
                    certAuthResponce.ResponseStatusMessage = statusMessage != null ? HttpUtility.HtmlDecode(statusMessage.InnerText) : string.Empty;
                    certAuthResponce.ResponseStatus = GetResponseStatusFromCode(statusCodeValue, certAuthResponce.ResponseStatusMessage);
                }

                if (certAuthResponce.ResponseStatus != Enums.eCertResponseStatus.Success)
                {
                    return certAuthResponce;
                }

                //successful result => get data
                var subjectNode = responseElement.SelectSingleNode("//saml2:Subject", samlNS);
                if (subjectNode != null)
                {
                    //get egn
                    var egn = subjectNode.SelectSingleNode("saml2:NameID[@NameQualifier='urn:egov:bg:eauth:1.0:attributes:eIdentifier:EGN']", samlNS);
                    if (egn != null)
                    {
                        certAuthResponce.EGN = egn.InnerText;
                        certAuthResponce.DateOfBirth = TextHelper.GetBirthDateFromEGN(certAuthResponce.EGN);
                    }
                }

                if (String.IsNullOrEmpty(certAuthResponce.EGN))
                {
                    ElmahLogger.Instance.Error("Can not extract egn from response! Can not continue process for authentication!");
                    certAuthResponce.ResponseStatus = Enums.eCertResponseStatus.MissingEGN;
                    return certAuthResponce;
                }

                //get phone and email if any
                var attributes = responseElement.SelectSingleNode("//saml2:AttributeStatement", samlNS);
                if (attributes != null)
                {
                    var phone = attributes.SelectSingleNode("saml2:Attribute[@Name='urn:egov:bg:eauth:1.0:attributes:phone']/saml2:AttributeValue", samlNS);
                    if (phone != null) certAuthResponce.PhoneNumber = phone.InnerText;
                    var email = attributes.SelectSingleNode("saml2:Attribute[@Name='urn:egov:bg:eauth:1.0:attributes:eMail']/saml2:AttributeValue", samlNS);
                    if (email != null) certAuthResponce.Email = email.InnerText;
                    var latinName = attributes.SelectSingleNode("saml2:Attribute[@Name='urn:egov:bg:eauth:1.0:attributes:personNamesLatin']/saml2:AttributeValue", samlNS);
                    if (latinName != null) certAuthResponce.LatinNames = latinName.InnerText;
                }

                return certAuthResponce;
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(ex, "eAuth response Parsing Error!");
                throw;
            }
        }


        /// <summary>
        /// Get response status from a resonse code in saml response
        /// </summary>
        /// <param name="statusCode">status code valud</param>
        /// <returns></returns>
        private static Enums.eCertResponseStatus GetResponseStatusFromCode(string statusCode, string statusMessage)
        {
            switch (statusCode)
            {
                case "urn:oasis:names:tc:SAML:2.0:status:AuthnFailed":
                    if (statusMessage.Trim().ToLower() == "отказан от потребител")
                        return Enums.eCertResponseStatus.CanceledByUser;
                    else
                        return Enums.eCertResponseStatus.AuthenticationFailed;
                case "urn:oasis:names:tc:SAML:2.0:status:Success":
                    return Enums.eCertResponseStatus.Success;
            }

            return Enums.eCertResponseStatus.AuthenticationFailed;
        }
        #endregion SAML10

        #region Shared

        /// <summary>
        /// Saml AuthnRequest to eAuthenticator
        /// </summary>
        /// <param name="returnURL"></param>
        /// <returns></returns>
        internal static XmlDocument GenerateKEPAuthnRequest(Models.CertAuthViewModel requestInfo)
        {
            SAML2.Saml20AuthnRequest req = new SAML2.Saml20AuthnRequest();
            req.ProtocolBinding = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST";
            req.Request.ProviderName = requestInfo.ProviderName;
            req.Request.Issuer.SPProvidedID = requestInfo.ProviderId;
            req.Request.Issuer.Value = requestInfo.ReturnUrl;
            req.Request.IssueInstant = DateTime.Now;
            req.Request.Destination = requestInfo.TargetUrl;
            req.Request.ForceAuthn = false;
            req.Request.IsPassive = false;
            req.Request.AssertionConsumerServiceUrl = requestInfo.ReturnUrl;

            System.Xml.XmlElement[] ext = GetExtensions(requestInfo.ExtServiceId, requestInfo.ExtProviderId);
            req.Request.Extensions = new SAML2.Schema.Protocol.Extensions();
            req.Request.Extensions.Any = ext;
            var namespaces = new XmlSerializerNamespaces(SamlSerialization.XmlNamespaces);

            XmlDocument resp = new XmlDocument();
            switch (SamlHelper.SamlConfiguration.SamlVersion)
            {
                case 1:
                    namespaces.Add("egovbga", "urn:bg:egov:eauth:1.0:saml:ext");
                    break;
                case 2:
                    req.Request.Issuer.Value = SAML2.Config.Saml2Config.Current.AllowedAudienceUris.FirstOrDefault();
                    req.Request.NameIdPolicy = new NameIdPolicy() { AllowCreate = true };
                    namespaces.Add("egovbga", "urn:bg:egov:eauth:2.0:saml:ext");
                    break;
            }

            resp = SamlSerialization.Serialize(req.Request, namespaces);

            //remove the declaration
            if (resp.FirstChild is XmlDeclaration)
            {
                resp.RemoveChild(resp.FirstChild);
            }

            return resp;
        }

        /// <summary>
        /// Generata saml AuthNRequest extensions
        /// </summary>
        /// <param name="service"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        private static System.Xml.XmlElement[] GetExtensions(string service, string provider)
        {
            var extensionStr = string.Empty;
            switch (SamlConfiguration.SamlVersion)
            {
                case 1:
                    extensionStr = String.Format(@"<egovbga:RequestedService><egovbga:Service>{0}</egovbga:Service><egovbga:Provider>{1}</egovbga:Provider></egovbga:RequestedService>", service, provider);
                    break;
                case 2:
                    StringBuilder sb = new StringBuilder();

                    sb.AppendFormat(@"<egovbga:RequestedService><egovbga:Service>{0}</egovbga:Service><egovbga:Provider>{1}</egovbga:Provider><egovbga:LevelOfAssurance>SUBSTANTIAL</egovbga:LevelOfAssurance></egovbga:RequestedService>", service, provider);
                    sb.Append(@"<egovbga:RequestedAttributes xmlns:egovbga=""urn:bg:egov:eauth:2.0:saml:ext"">");
                    sb.Append(@"<egovbga:RequestedAttribute FriendlyName=""latinName"" Name=""urn:egov:bg:eauth:2.0:attributes:latinName"" NameFormat=""urn:oasis:names:tc:saml2:2.0:attrname-format:uri"" isRequired=""true"">");
                    sb.Append(@"<egovbga:AttributeValue>urn:egov:bg:eauth:2.0:attributes:latinName</egovbga:AttributeValue></egovbga:RequestedAttribute>");
                    sb.Append(@"<egovbga:RequestedAttribute FriendlyName=""birthName"" Name=""urn:egov:bg:eauth:2.0:attributes:birthName"" NameFormat=""urn:oasis:names:tc:saml2:2.0:attrname-format:uri"" isRequired=""true"">");
                    sb.Append(@"<egovbga:AttributeValue>urn:egov:bg:eauth:2.0:attributes:birthName</egovbga:AttributeValue></egovbga:RequestedAttribute>");
                    sb.Append(@"<egovbga:RequestedAttribute FriendlyName=""email"" Name=""urn:egov:bg:eauth:2.0:attributes:email"" NameFormat=""urn:oasis:names:tc:saml2:2.0:attrname-format:uri""	isRequired=""true"">");
                    sb.Append(@"<egovbga:AttributeValue>urn:egov:bg:eauth:2.0:attributes:email</egovbga:AttributeValue></egovbga:RequestedAttribute>");
                    sb.Append(@"<egovbga:RequestedAttribute FriendlyName=""phone"" Name=""urn:egov:bg:eauth:2.0:attributes:phone"" NameFormat=""urn:oasis:names:tc:saml2:2.0:attrname-format:uri"" isRequired=""true"">");
                    sb.Append(@"<egovbga:AttributeValue>urn:egov:bg:eauth:2.0:attributes:phone</egovbga:AttributeValue></egovbga:RequestedAttribute>");
                    sb.Append(@"<egovbga:RequestedAttribute FriendlyName=""dateOfBirth"" Name=""urn:egov:bg:eauth:2.0:attributes:dateOfBirth"" NameFormat=""urn:oasis:names:tc:saml2:2.0:attrname-format:uri"" isRequired=""false"">");
                    sb.Append(@"<egovbga:AttributeValue>urn:egov:bg:eauth:2.0:attributes:dateOfBirth</egovbga:AttributeValue></egovbga:RequestedAttribute>");
                    sb.Append(@"</egovbga:RequestedAttributes>");
                    extensionStr = sb.ToString();
                    break;
            }

            var doc = new System.Xml.XmlDocument();
            using (var sr = new System.IO.StringReader($"<root>{extensionStr}</root>"))
            using (var xtr = new System.Xml.XmlTextReader(sr) { Namespaces = false })
            {
                doc.Load(xtr);
            }
            return doc.DocumentElement.ChildNodes.Cast<XmlElement>().ToArray();
        }

        #endregion Shared


        #region SAML20


        /// <summary>
        /// Generate SP metadata
        /// </summary>
        /// <returns></returns>
        public static string GetSPMetdata()
        {
            var keyinfo = new KeyInfo();
            var keyClause = new KeyInfoX509Data(SAML2.Config.Saml2Config.Current.ServiceProvider.SigningCertificate.GetCertificate(), X509IncludeOption.EndCertOnly);
            keyinfo.AddClause(keyClause);

            var doc = new Saml20MetadataDocument(SAML2.Config.Saml2Config.Current, keyinfo, true);

            return doc.ToXml(Encoding.UTF8);
        }

        /// <summary>
        /// Parse saml2.0 response
        /// </summary>
        /// <param name="decodedStr"></param>
        /// <returns></returns>
        internal static CertificateAuthResponse ParseSaml2CertificateResultV2(string decodedStr, bool validateDate = true)
        {
            var response = new CertificateAuthResponse()
            {
                ResponseStatus = eCertResponseStatus.Success
            };
            var responseV2 = SAML2.Utils.Serialization.DeserializeFromXmlString<SAML2.Schema.Protocol.Response>(decodedStr);
            if (responseV2 == null)
            {
                response.ResponseStatus = eCertResponseStatus.InvalidResponseXML;
                return response;
            }

            if (responseV2.Status.StatusCode.Value != Saml20Constants.StatusCodes.Success)
            {
                logger.Error("Saml20 status not Success: Status code is: " + responseV2.Status.StatusCode.Value);
                response.ResponseStatus = eCertResponseStatus.AuthenticationFailed;
                response.ResponseStatusMessage = responseV2.Status.StatusMessage ?? responseV2.Status.StatusCode.Value;
                return response;
            }

            Saml20Assertion assertation = null;//responseV2.Items[0] as SAML2.Schema.Core.Assertion;
            //validation
            if (!ValidateAssertationSignature(decodedStr, out assertation))
            {
                logger.Error("Saml20 signature can not be validated!");
                response.ResponseStatus = eCertResponseStatus.InvalidSignature;
                return response;
            }

            var nw = DateTime.UtcNow;
            if (validateDate && assertation.Conditions?.NotOnOrAfter != null && assertation.Conditions.NotOnOrAfter.Value < nw)
            {
                logger.Error($"SAML2 parse response: assertation.Conditions.NotOnOrAfter value is {assertation.Conditions.NotOnOrAfter.Value} is exceeded by current time: {nw}");
                //expired response
                response.ResponseStatus = eCertResponseStatus.InvalidResponseXML;
                return response;
            }
            var attributes = assertation.Attributes;
            var personalIdentifier = attributes.SingleOrDefault(x => x.Name == "urn:egov:bg:eauth:2.0:attributes:personIdentifier")?.AttributeValue?.FirstOrDefault();
            if (!string.IsNullOrEmpty(personalIdentifier))
            {
                response.EGN = personalIdentifier.Split(new char[] { '-', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).Last();
            }
            if (string.IsNullOrEmpty(response.EGN))
            {
                response.ResponseStatus = eCertResponseStatus.MissingEGN;
                return response;
            }

            var names = attributes.SingleOrDefault(x => x.Name == "urn:egov:bg:eauth:2.0:attributes:personName")?.AttributeValue?.FirstOrDefault();
            if (!string.IsNullOrEmpty(names))
            {
                response.LatinNames = names;
            }


            var email = attributes.SingleOrDefault(x => x.Name == "urn:egov:bg:eauth:2.0:attributes:email")?.AttributeValue?.FirstOrDefault();
            if (!string.IsNullOrEmpty(email))
            {
                response.Email = email;
            }


            var phone = attributes.SingleOrDefault(x => x.Name == "urn:egov:bg:eauth:2.0:attributes:phone")?.AttributeValue?.FirstOrDefault();
            if (!string.IsNullOrEmpty(phone))
            {
                response.PhoneNumber = phone;
            }

            var dob = attributes.SingleOrDefault(x => x.Name == "urn:egov:bg:eauth:2.0:attributes:dateOfBirth")?.AttributeValue?.FirstOrDefault();
            if (!string.IsNullOrEmpty(dob))
            {
                response.DateOfBirth = DateTime.Parse(dob);
            }
            return response;
        }

        /// <summary>
        /// Validate SAML2 assertation signature
        /// </summary>
        /// <param name="decodedStr"></param>
        /// <returns></returns>
        private static bool ValidateAssertationSignature(string decodedStr, out Saml20Assertion assertion)
        {
            assertion = null;
            var assertionElement = ExtractAssertion(decodedStr);
            if (assertionElement == null)
            {
                logger.Error("Can not extract asserion element from " + decodedStr);
                return false;
            }

            var issuer = GetIssuer(assertionElement);
            var endpoint = RetrieveIDPConfiguration(issuer);
            assertion = new Saml20Assertion(assertionElement, null, SAML2.Config.Saml2Config.Current.AssertionProfile.AssertionValidator, false);

            if (assertion != null)
            {
                var metadataHelper = new MetaDataHelper(logger);
                metadataHelper.InitIdPMetadata(endpoint);
                if (endpoint.Metadata == null)
                {
                    return false;
                }

                var trustedSigners = GetTrustedSigners(endpoint.Metadata.GetKeys(SAML2.Schema.Metadata.KeyTypes.Signing), endpoint);
                return assertion.CheckSignature(trustedSigners);
            }

            logger.Error("Can not create assertion object from assertionElement " + assertionElement.OuterXml);
            return false;
        }


        /// <summary>
        /// Extract assertion element from 
        /// </summary>
        /// <param name="decodedStr"></param>
        /// <returns></returns>
        private static XmlElement ExtractAssertion(string decodedStr)
        {
            var doc = new XmlDocument { PreserveWhitespace = true };
            doc.LoadXml(decodedStr);

            var assertionList = doc.DocumentElement.GetElementsByTagName(SAML2.Schema.Core.Assertion.ElementName, Saml20Constants.Assertion);
            if (assertionList.Count == 1)
            {
                var assertionElement = (XmlElement)assertionList[0];
                return assertionElement;
            }

            //try encrypted
            var encryptedAssList = doc.GetElementsByTagName(EncryptedAssertion.ElementName, Saml20Constants.Assertion);
            if (encryptedAssList.Count == 1)
            {
                var privateKey = (RSA)SAML2.Config.Saml2Config.Current.ServiceProvider.SigningCertificate.GetCertificate().PrivateKey;

                var encryptedAssertion = new Saml20EncryptedAssertion(privateKey);
                encryptedAssertion.LoadXml((XmlElement)encryptedAssList[0]);
                // Act
                encryptedAssertion.Decrypt();
                return encryptedAssertion.Assertion.DocumentElement;
            }

            return null;
        }



        /// <summary>
        /// Retrieves the name of the issuer from an XmlElement containing an assertion.
        /// </summary>
        /// <param name="assertion">An XmlElement containing an assertion</param>
        /// <returns>The identifier of the Issuer</returns>
        private static string GetIssuer(XmlElement assertion)
        {
            var result = string.Empty;
            var list = assertion.GetElementsByTagName("Issuer", Saml20Constants.Assertion);
            if (list.Count > 0)
            {
                var issuer = (XmlElement)list[0];
                result = issuer.InnerText;
            }

            return result;
        }

        /// <summary>
        /// Looks through the Identity Provider configurations and
        /// </summary>
        /// <param name="idpId">The identity provider id.</param>
        /// <returns>The <see cref="IdentityProviderElement"/>.</returns>
        private static SAML2.Config.IdentityProvider RetrieveIDPConfiguration(string idpId)
        {
            var config = Saml2Section.GetConfig();
            return config.IdentityProviders.FirstOrDefault(x => x.Id == idpId);
        }

        /// <summary>
        /// Get trusted signers
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="identityProvider"></param>
        /// <returns></returns>
        private static IEnumerable<AsymmetricAlgorithm> GetTrustedSigners(ICollection<SAML2.Schema.Metadata.KeyDescriptor> keys, SAML2.Config.IdentityProvider identityProvider)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            var result = new List<AsymmetricAlgorithm>(keys.Count);
            foreach (var keyDescriptor in keys)
            {
                foreach (KeyInfoClause clause in (KeyInfo)keyDescriptor.KeyInfo)
                {
                    var key = XmlSignatureUtils.ExtractKey(clause);
                    result.Add(key);
                }
            }

            return result;
        }



        /// <summary>
        /// Compress binary data with gZip compression
        /// </summary>
        /// <param name="data">bynary data</param>
        /// <returns>compressed data</returns>
        public static byte[] CompressDeflate(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                throw new ArgumentNullException("data");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (System.IO.Compression.DeflateStream def = new System.IO.Compression.DeflateStream(ms, CompressionMode.Compress, true))
                {
                    def.Write(data, 0, data.Length);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// DeCompress compressed  with gZip data 
        /// </summary>
        /// <param name="data">compressed data</param>
        /// <returns>decompressed data</returns>
        public static byte[] DeCompressDeflate(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length == 0)
            {
                throw new ArgumentNullException("compressedData");
            }

            MemoryStream msOut;
            MemoryStream msIn;
            byte[] buffer = new byte[1024];
            using (msIn = new MemoryStream(compressedData))
            using (msOut = new MemoryStream())
            {
                using (System.IO.Compression.DeflateStream def = new System.IO.Compression.DeflateStream(msIn, CompressionMode.Decompress))
                {
                    int bytesRead = 0;
                    while ((bytesRead = def.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        msOut.Write(buffer, 0, bytesRead);
                    }
                }
                return msOut.ToArray();
            }
        }
        #endregion SAML20
    }


    /// <summary>
    /// Metadata fetcher module
    /// </summary>
    internal class MetaDataHelper
    {
        private ILog logger;
        public MetaDataHelper(ILog logParam)
        {
            logger = logParam;
        }

        /// <summary>
        /// InutIdpMetadata
        /// </summary>
        /// <param name="endpoint"></param>
        internal void InitIdPMetadata(IdentityProvider endpoint)
        {

            var key = $"Metadata_{endpoint.Id}";
            Saml20MetadataDocument metadata = (Saml20MetadataDocument)HttpContext.Current.Cache[key];
            if (metadata == null || (metadata.Entity.ValidUntil.HasValue && metadata.Entity.ValidUntil < DateTime.UtcNow))
            {
                metadata = LoadIdPMetadata(endpoint.Endpoints.SingleOrDefault(x => x.Type == EndpointType.Metadata).Url);
                if (metadata == null)
                {
                    logger.Error("Load IPD metadata was not successful. Endpoint id: " + endpoint.Id);
                    return;
                }

                HttpContext.Current.Cache.Add(key, metadata, null, (metadata.Entity.ValidUntil ?? DateTime.UtcNow.AddDays(1)), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, null);
            }
            endpoint.Metadata = metadata;
        }

        /// <summary>
        /// Load idp metadata from url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private Saml20MetadataDocument LoadIdPMetadata(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var uri = new Uri(url);
                    var metadataStr = client.GetStringAsync(uri).Result;

                    var doc = new XmlDocument { PreserveWhitespace = true };
                    doc.LoadXml(metadataStr);

                    logger.Info($"Loaded metadata for url {url}: metadata:{metadataStr}");
                    // init metadata
                    var metadata = new Saml20MetadataDocument(doc);
                    return metadata;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Can not load IDP metadata from url " + url, ex);
                return null;
            }
        }

    }

}
