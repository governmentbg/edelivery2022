using System;
using System.Globalization;
using System.Xml;

using log4net;

using RegixInfoClient.DataContracts;
using RegixInfoClient.RegiXEntryPoint;

namespace RegixInfoClient
{
    public class RegixClient : IRegixClient
    {
        private static ILog logger = LogManager.GetLogger("RegixEntryPoint");
        private static string RegixDateFormat = "yyyy-MM-dd";
        /// <summary>
        /// Get Valid Person inof
        /// </summary>
        /// <param name="egn"></param>
        /// <returns></returns>
        public ValidPersonResponse GetValidPersonInfo(string egn)
        {
            logger.Info($"Sending ValidPersonInfo request for EGN {egn}");
            var response = new ValidPersonResponse() { Success = false };
            try
            {
                using (var client = new RegiXEntryPointClient())
                {
                    logger.Info("Regix client initiated successfully");

                    var serviceOP = "TechnoLogica.RegiX.GraoNBDAdapter.APIService.INBDAPI.ValidPersonSearch";
                    string requestXml = $@"<ValidPersonRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
                                                xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
                                                xmlns=""http://egov.bg/RegiX/GRAO/NBD/ValidPersonRequest"">
                                                <EGN>{egn}</EGN>
                                            </ValidPersonRequest>";
                    var xDoc = new XmlDocument();
                    xDoc.LoadXml(requestXml);
                    var responseElement = SendRegixRequest(client, serviceOP, xDoc.DocumentElement);
                    if (responseElement != null)
                    {
                        if (responseElement.HasError)
                        {
                            response.ErrorMessage = responseElement.Error;
                            return response;
                        }

                        if (responseElement.Data.Response?.Any != null)
                        {
                            var xmlItem = responseElement.Data.Response.Any;
                            //success
                            response.Success = ParseXmlResponse(xmlItem, response);
                        }
                        return response;
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetValidPersonInfo", ex);
            }
            return null;
        }

        #region Private methods

        /// <summary>
        /// Parse valid person response info to
        /// </summary>
        /// <param name="responseElement"></param>
        /// <param name="response"></param>
        public bool ParseXmlResponse(XmlElement xmlItem, ValidPersonResponse response)
        {
            try
            {
                var ti = CultureInfo.InvariantCulture.TextInfo;
                if (xmlItem != null)
                {
                    response = response ?? new ValidPersonResponse();
                    response.Success = true;
                    response.FirstName = ti.ToTitleCase(ti.ToLower(xmlItem.GetElementsByTagName("FirstName").Item(0).InnerText));
                    response.SurName = ti.ToTitleCase(ti.ToLower(xmlItem.GetElementsByTagName("SurName")?.Item(0)?.InnerText));
                    response.FamilyName = ti.ToTitleCase(ti.ToLower(xmlItem.GetElementsByTagName("FamilyName")?.Item(0)?.InnerText));
                    string dateOfBirth = xmlItem.GetElementsByTagName("BirthDate")?.Item(0)?.InnerText;
                    if (!string.IsNullOrEmpty(dateOfBirth))
                    {
                        response.BirthDate = DateTime.ParseExact(dateOfBirth, RegixDateFormat, CultureInfo.InvariantCulture);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Error parsing xml Element response: " + xmlItem.OuterXml, ex);
                return false;
            }
        }
        /// <summary>
        /// Send regix request
        /// </summary>
        /// <param name="client"></param>
        /// <param name="operation"></param>
        /// <param name="requestElement"></param>
        /// <returns></returns>
        private ServiceResultData SendRegixRequest(RegiXEntryPointClient client, string operation, XmlElement requestElement)
        {
            try
            {
                logger.InfoFormat("In SendRegixRequest for op {0}, requestElement: {1}", operation, requestElement.OuterXml);
                RegiXEntryPoint.ServiceRequestData request = new RegiXEntryPoint.ServiceRequestData();
                // Име на операцията, която искаме да изпълним
                request.Operation = operation;
                // Контекст (описание), в който изпълняваме заявката
                request.CallContext = new CallContext()
                {
                    // Име на администрацията създала заявката
                    AdministrationName = RegixConfiguration.AdministrationName,
                    // OID на администрацията създала заявката
                    AdministrationOId = RegixConfiguration.AdministrationOid,
                    // Идентификатор на служителя създал заявката
                    //EmployeeIdentifier = "<employee_identifier>",
                    // Допълнителене идентификатор на служителя създал заявката
                    //EmployeeAditionalIdentifier = "<employee_additional_identifier>",
                    // Имена на служителя създал заявката
                    //EmployeeNames = "<employee_names>",
                    // Позиция на служителя създал заявката
                    //EmployeePosition = "<employee_position>",
                    // Правно основание
                    LawReason = RegixConfiguration.LawReason,
                    // Пояснения
                    Remark = RegixConfiguration.Remark,
                    // Идентификатор на отговорният служител
                    ResponsiblePersonIdentifier = RegixConfiguration.ResponsiblePerson,
                    // Тип на услугата
                    ServiceType = RegixConfiguration.ServiceType,
                    // URI на услугата
                    ServiceURI = RegixConfiguration.ServiceURI,
                };
                request.SignResult = false;
                request.ReturnAccessMatrix = false;
                // XmlElement съдържащ аргумента на заявката
                request.Argument = requestElement;
                // Изпълнение на услугата. Резултатът се съдържа в променливата result
                logger.InfoFormat("Calling regixEntryPoint ExecuteSynchronous. Request string:  {0}", Newtonsoft.Json.JsonConvert.SerializeObject(request));
                RegiXEntryPoint.ServiceResultData result = client.ExecuteSynchronous(request);
                logger.InfoFormat("Calling regixEntryPoint ExecuteSynchronous. Result string:  {0}", Newtonsoft.Json.JsonConvert.SerializeObject(result));
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Error calling regixEntryPoint ExecuteSynchronous", ex);
            }

            return null;

        }
        #endregion Private methods

    }
}
