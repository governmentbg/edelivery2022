using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using EDelivery.Common.DataContracts;
using EDelivery.Common.DataContracts.ESubject;
using EDelivery.Common.Enums;

namespace ED.IntegrationService
{
    [ServiceContract(Namespace = "https://edelivery.egov.bg/services/integration")]
    public interface IEDeliveryIntegrationService
    {
        [OperationContract]
        Task<List<DcInstitutionInfo>> GetRegisteredInstitutions();

        [OperationContract]
        Task<int> SendElectronicDocument(
            string subject,
            byte[] docBytes,
            string docNameWithExtension,
            string docRegNumber,
            eProfileType receiverType,
            string receiverUniqueIdentifier,
            string receiverPhone,
            string receiverEmail,
            string serviceOID,
            string operatorEGN);

        [OperationContract]
        Task<int> SendElectronicDocumentWithAccessCode(
            string subject,
            byte[] docBytes,
            string docNameWithExtension,
            string docRegNumber,
            DcMessageWithCodeReceiver receiver,
            string serviceOID,
            string operatorEGN);

        [OperationContract]
        Task<int> SendElectronicDocumentOnBehalfOf(
            string subject,
            byte[] docBytes,
            string docNameWithExtension,
            string docRegNumber,
            eProfileType senderType,
            string senderUniqueIdentifier,
            string senderPhone,
            string senderEmail,
            string senderFirstName,
            string senderLastName,
            eProfileType receiverType,
            string receiverUniqueIdentifier,
            string serviceOID,
            string operatorEGN);

        [OperationContract]
        Task<int> SendMessage(
            DcMessageDetails message,
            eProfileType receiverType,
            string receiverUniqueIdentifier,
            string receiverPhone,
            string receiverEmail,
            string serviceOID,
            string operatorEGN);

        [OperationContract]
        Task<int> SendMessageWithAccessCode(
            DcMessageDetails message,
            DcMessageWithCodeReceiver receiver,
            string serviceOID,
            string operatorEGN);

        [OperationContract]
        Task<int> SendMessageInReplyTo(
            DcMessageDetails message,
            int replyToMessageId,
            string serviceOID,
            string operatorEGN);

        [OperationContract]
        Task<int> SendMessageOnBehalfOf(
           DcMessageDetails message,
           eProfileType senderType,
           string senderUniqueIdentifier,
           string senderPhone,
           string senderEmail,
           string senderFirstName,
           string senderLastName,
           eProfileType receiverType,
           string receiverUniqueIdentifier,
           string serviceOID,
           string operatorEGN);

        [OperationContract]
        Task<DcMessageDetails> GetSentDocumentStatusByRegNum(
            string documentRegistrationNumber,
            string operatorEGN);

        [OperationContract]
        Task<DcDocument> GetSentDocumentContentByRegNum(
            string documentRegistrationNumber,
            string operatorEGN);

        [OperationContract]
        Task<List<DcDocument>> GetSentDocumentsContent(
            int messageId,
            string operatorEGN);

        [OperationContract]
        Task<DcDocument> GetSentDocumentContent(
            int documentId,
            string operatorEGN);

        [OperationContract]
        Task<DcMessageDetails> GetSentMessageStatus(
            int messageId,
            string operatorEGN);

        [OperationContract]
        Task<List<DcMessage>> GetSentMessagesList(string operatorEGN);

        [OperationContract]
        Task<DcPartialList<DcMessage>> GetSentMessagesListPaged(
            int pageNumber,
            int pageSize,
            string operatorEGN);

        [OperationContract]
        Task<List<DcMessage>> GetReceivedMessagesList(
            bool onlyNew,
            string operatorEGN);

        [OperationContract]
        Task<DcPartialList<DcMessage>> GetReceivedMessagesListPaged(
            bool onlyNew,
            int pageNumber,
            int pageSize,
            string operatorEGN);

        [OperationContract]
        Task<DcMessageDetails> GetSentMessageContent(
            int messageId,
            string operatorEGN);

        [OperationContract]
        Task<DcMessageDetails> GetReceivedMessageContent(
            int messageId,
            string operatorEGN);

        [OperationContract]
        Task<DcPersonRegistrationInfo> CheckPersonHasRegistration(
            string personId);

        [OperationContract]
        Task<DcLegalPersonRegistrationInfo> CheckLegalPersonHasRegistration(
            string eik);

        [OperationContract]
        Task<DcSubjectRegistrationInfo> CheckSubjectHasRegistration(
            string identificator);

        [OperationContract]
        Task<DcSubjectInfo> GetSubjectInfo(
            Guid electronicSubjectId,
            string operatorEGN);

        [OperationContract]
        Task<DcStatisticsGeneral> GetEDeliveryGeneralStatistics();

        [OperationContract]
        Task<int> SendMessageOnBehalfToPerson(
          DcMessageDetails message,
          string senderUniqueIdentifier,
          string receiverUniqueIdentifier,
          string receiverPhone,
          string receiverEmail,
          string receiverFirstName,
          string receiverLastName,
          string serviceOID,
          string operatorEGN);

        [OperationContract]
        Task<int> SendMessageOnBehalfToLegalEntity(
          DcMessageDetails message,
          string senderUniqueIdentifier,
          string receiverUniqueIdentifier,
          string serviceOID,
          string operatorEGN);
    }
}
