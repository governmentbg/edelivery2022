using System;
using System.Net;
using System.Xml;
using log4net;
using EDelivery.SEOS.DataContracts;

namespace EDelivery.SEOS.Utils
{
    public class EndPointHelper
    {
        /// <summary>
        /// Validate the endpoint response is signed with the correct certificate
        /// </summary>
        /// <param name="result"></param>
        /// <param name="certificateSN"></param>
        /// <returns></returns>
        public static bool ValidateEndpointResponse(string result, ILog logger)
        {
            try
            {
                string certNumber;
                XmlDocument resultDoc = new XmlDocument();
                resultDoc.LoadXml(result);
                return SignedXmlHelper.ValidateXmlSignature(resultDoc, out certNumber);
            }
            catch (Exception ex)
            {
                logger.Error("Can not validate endpoint response! Invalid signature", ex);
                return false;
            }
        }

        /// <summary>
        /// Submit endpoint message
        /// </summary>
        /// <param name="serviceUrl">endpoint url</param>
        /// <param name="seosMessage">seos message</param>
        /// <returns></returns>
        public static SendMessageResult SubmitEndpointMessage(string serviceUrl, string seosMessage, ILog logger)
        {
            try
            {
                logger.Info("Submit endpoint message to serviceUrl: " + serviceUrl);

                var xsdValidate = new XsdValidationHelper(seosMessage, logger);
                xsdValidate.Validate();

                using (EGovEndpoint.EGovServiceClient endpointClient = new EGovEndpoint.EGovServiceClient())
                {
                    endpointClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(serviceUrl);
                    var result = endpointClient.Submit(seosMessage);
                    //log the result in db
                    logger.Info("Submit endpoint message is successful, result is " + result);
                    return new SendMessageResult
                    {
                        ErrorMessage = String.Empty,
                        Status = DocumentStatusType.DS_WAIT_REGISTRATION,
                        Request = result
                    };
                }
            }
            catch (WebException e1)
            {
                return new SendMessageResult
                {
                    ErrorMessage = e1.Message,
                    Status = DocumentStatusType.DS_TRY_SEND,
                    Request = String.Empty
                };
            }
            catch (System.ServiceModel.EndpointNotFoundException e2)
            {
                return new SendMessageResult
                {
                    ErrorMessage = e2.Message,
                    Status = DocumentStatusType.DS_TRY_SEND,
                    Request = String.Empty
                };
            }
            catch (System.ServiceModel.CommunicationException e3)
            {
                return new SendMessageResult
                {
                    ErrorMessage = e3.Message,
                    Status = DocumentStatusType.DS_SENT_FAILED,
                    Request = String.Empty
                };
            }
            catch (Exception ex)
            {
                return new SendMessageResult
                {
                    ErrorMessage = ex.Message,
                    Status = DocumentStatusType.DS_TRY_SEND,
                    Request = String.Empty
                };
            }
        }
    }
}
