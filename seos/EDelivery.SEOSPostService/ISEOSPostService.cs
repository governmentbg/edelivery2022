using System;
using System.Collections.Generic;
using System.ServiceModel;
using EDelivery.SEOS.DataContracts;

namespace EDelivery.SEOSPostService
{
    [ServiceContract]
    public interface ISEOSPostService
    {
        [OperationContract]
        SendMessageResult SendSeos(string url, string message);

        [OperationContract]
        SendMessageResult SendAs4(string url, string message);

        [OperationContract]
        MessagesPageResponse GetReceivedDocuments(Guid eSubjectId, int page, int pageSize, SortColumnEnum sortColumn, SortOrderEnum sortOrder);

        [OperationContract]
        MessagesPageResponse GetSentDocuments(Guid eSubjectId, int page, int pageSize, SortColumnEnum sortColumn, SortOrderEnum sortOrder);

        [OperationContract]
        Dictionary<Guid, int> GetNewMessagesCount(List<Guid> profileGuidList);

        [OperationContract]
        OpenDocumentResponse OpenDocument(OpenDocumentRequest request);

        [OperationContract]
        List<RegisteredEntityResponse> GetRegisteredEntities();

        [OperationContract]
        RegisteredEntityResponse GetEDeliveryEntity();

        [OperationContract]
        RegisteredEntityResponse GetRegisteredEntity(Guid entityId);

        [OperationContract]
        Dictionary<string, bool> HasSeos(List<string> uicList);

        [OperationContract]
        DocumentStatusType? CheckDocumentStatus(Guid messageId, Guid electronicSubjectId);

        [OperationContract]
        ChangeDocumentStatusResponse GetDocumentStatus(Guid messageId, Guid eSubjectId);

        [OperationContract]
        bool UpdateDocumentStatus(ChangeDocumentStatusRequest request);

        [OperationContract]
        AttachmentResponse GetDocumentAttachment(Guid? messageGuid, int attachmentId);

        [OperationContract]
        MalwareScanResultResponse SaveAttachmentWithScan(AttachmentRequest doc);

        [OperationContract]
        SubmitStatusRequestResult SendMessage(MessageRequest request, CorespondentRequest corespondent);
    }
}
