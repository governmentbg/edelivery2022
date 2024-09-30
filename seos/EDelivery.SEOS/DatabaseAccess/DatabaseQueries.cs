using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.DBEntities;
using EDelivery.SEOS.EDeliveryAS4Node;
using EDelivery.SEOS.EGovRegstry;
using EDelivery.SEOS.Models;
using EDelivery.SEOS.Utils;

namespace EDelivery.SEOS.DatabaseAccess
{
    public class DatabaseQueries
    {
        /// <summary>
        /// Get all messages sent to profile in EDelivery
        /// </summary>
        /// <param name="eSubjectId">Profile Id in EDelivery</param>
        /// <param name="pageNum">Page number</param>
        /// <param name="pageSize">Page Size</param>
        /// <param name="column">Sort column</param>
        /// <param name="descending">Sort order</param>
        /// <returns>Page of messages</returns>
        public static MessagesPage GetSentDocuments(Guid eSubjectId, int pageNum, int pageSize, SortColumnEnum column, bool descending)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var result = dbContext.SEOSMessages
                    .Include(x => x.Sender)
                    .Include(x => x.Receiver)
                    .Where(x => x.SenderElectronicSubjectId == eSubjectId);
                var count = result.Count();
                result = SortResult.Sort(result, column, descending);

                return new MessagesPage
                {
                    Messages = result
                    .Skip(pageSize * (pageNum - 1))
                    .Take(pageSize).ToList(),
                    CountAllMessages = count
                };
            }
        }

        /// <summary>
        /// Get all messages received for a profile in EDelivery
        /// </summary>
        /// <param name="eSubjectId">Profile Id in EDelivery</param>
        /// <param name="pageNum">Page number</param>
        /// <param name="pageSize">Page Size</param>
        /// <param name="column">Sort column</param>
        /// <param name="descending">Sort order</param>
        /// <returns>Page of messages</returns>
        public static MessagesPage GetReceivedDocuments(Guid eSubjectId, int pageNum, int pageSize, SortColumnEnum column, bool descending)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var result = dbContext.SEOSMessages
                    .Include(x => x.Sender)
                    .Include(x => x.Receiver)
                    .Where(x => x.ReceiverElectronicSubjectId == eSubjectId);
                var count = result.Count();
                result = SortResult.Sort(result, column, descending);
                return new MessagesPage
                {
                    Messages = result
                    .Skip(pageSize * (pageNum - 1))
                    .Take(pageSize).ToList(),
                    CountAllMessages = count
                };
            }
        }

        /// <summary>
        /// Get registered entities
        /// </summary>
        /// <returns></returns>
        public static List<RegisteredEntity> GetRegisteredEntities()
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                return dbContext.RegisteredEntities.ToList();
            }
        }

        public static RegisteredEntity GetEDeliveryEntity()
        {
            var eDeliveryEntitId = Guid.Parse(WebConfigurationManager.AppSettings["SEOS.EDeliveryGUID"]);

            return GetRegisteredEntity(eDeliveryEntitId);
        }

        public static RegisteredEntity GetRegisteredEntity(Guid entityId)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                return dbContext.RegisteredEntities.SingleOrDefault(x => x.UniqueId == entityId);
            }
        }

        public static RegisteredEntity GetRegisteredEntity(string eik)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                return dbContext.RegisteredEntities.FirstOrDefault(x => x.EIK == eik);
            }
        }

        public static RegisteredEntity GetRegisteredEntityInEDelivery(string uic)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;
            var eDeliveryEntity = GetEDeliveryEntity();

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                return dbContext.RegisteredEntities
                    .SingleOrDefault(x => x.EIK == uic &&
                                     x.Status != 0 &&
                                     x.CertificateSN == eDeliveryEntity.CertificateSN &&
                                     x.ServiceUrl.ToLower() == eDeliveryEntity.ServiceUrl.ToLower());
            }
        }

        public static bool UpdateRegisteredEntities(IEnumerable<Entity> entities)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var dbEntities = dbContext.RegisteredEntities.ToList();

                foreach (var entitiy in entities)
                {
                    var dbEntity = dbEntities.FirstOrDefault(x => x.UniqueId == Guid.Parse(entitiy.Guid));
                    if (dbEntity == null)
                    {
                        dbEntity = new RegisteredEntity()
                        {
                            EIK = entitiy.EntityIdentifier,
                            UniqueId = Guid.Parse(entitiy.Guid)
                        };
                        dbContext.RegisteredEntities.Add(dbEntity);
                    }

                    dbEntity.Name = entitiy.AdministrativeBodyName;
                    dbEntity.CertificateSN = entitiy.CertificateSN;
                    dbEntity.Email = entitiy.Contact.EmailAddress;
                    dbEntity.Fax = entitiy.Contact.Fax;
                    dbEntity.Phone = entitiy.Contact.Phone;
                    dbEntity.LastChange = entitiy.LastChange;
                    dbEntity.DateUpdated = DateTime.Now;

                    var entityService = RegisteredEntitiesHelper.GetEntityService(entitiy);
                    dbEntity.ServiceUrl = entityService?.URI;
                    dbEntity.Status = (int)(entitiy.Status == eEntityServiceStatus.Active.ToString()
                        ? Enum.Parse(typeof(eEntityServiceStatus), entityService?.Status ?? "Inactive")
                        : Enum.Parse(typeof(eEntityServiceStatus), entitiy.Status));
                }

                var existingEntities = entities.ToDictionary(p => Guid.Parse(p.Guid), p => p);
                dbEntities.ToList().ForEach(x =>
                {
                    if (!existingEntities.ContainsKey(x.UniqueId))
                    {
                        x.Status = (int)eEntityServiceStatus.Inactive;
                    }
                });

                dbContext.SaveChanges();
                return true;
            }
        }

        public static bool HasEDeliverySeos(string uic)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;
            var eDeliveryEntity = GetEDeliveryEntity();

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                return dbContext.RegisteredEntities
                    .Any(x => x.EIK == uic &&
                         x.Status == 1 &&
                         x.CertificateSN == eDeliveryEntity.CertificateSN &&
                         x.ServiceUrl.ToLower() == eDeliveryEntity.ServiceUrl.ToLower());
            }
        }

        public static Dictionary<string, bool> HasEDeliverySeos(List<string> uicList)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;
            var eDeliveryEntity = GetEDeliveryEntity();

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var nativeEntities = dbContext.RegisteredEntities
                    .Where(x => uicList.Contains(x.EIK) &&
                      x.Status == 1 &&
                      x.CertificateSN == eDeliveryEntity.CertificateSN)
                    .ToArray();

                return nativeEntities
                    .Where(x => !IsAS4Entity(x.EIK))
                    .ToDictionary(p => p.EIK, p => true);
            }
        }

        public static bool HasOldSeos(string eik)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                return dbContext.RegisteredEntities
                    .Any(x => x.EIK == eik && x.Status != 0);
            }
        }

        public static string GetSeosServiceUrl(string eik)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var entity = dbContext.RegisteredEntities
                    .FirstOrDefault(x => x.EIK == eik && x.Status != 0);

                return entity != null
                ? entity.ServiceUrl
                : String.Empty;
            }
        }

        public static SEOSMessage ApplySentDocumentStatus(int messageId, SubmitStatusRequestResult result)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var message = dbContext.SEOSMessages.Find(messageId);

                if (!String.IsNullOrEmpty(result.Error) ||
                    result.StatusResponse == null ||
                    result.StatusResponse.DocID == null)
                {
                    message.Status = (int)result.Status;
                    dbContext.SaveChanges();
                    return message;
                }

                DocumentStatusResponseType messageBody = result.StatusResponse;

                message.Status = (int)messageBody.DocRegStatus;
                if (messageBody.DocID != null)
                {
                    var numdate = messageBody.DocID.GetDocNumberAndDate(message.DocDateExternal);
                    message.DocNumberExternal = numdate.Item1;
                    message.DocDateExternal = numdate.Item2;
                };
                message.DocExpectCloseDate = messageBody.DocExpectCloseDateSpecified
                    ? messageBody.DocExpectCloseDate
                    : message.DocExpectCloseDate;

                message.DateUpdated = DateTime.Now;
                message.RejectedReason = messageBody.RejectionReason;

                dbContext.SaveChanges();

                var record = new SEOSStatusHistory()
                {
                    DateCreated = DateTime.Now,
                    MessageId = messageId,
                    UpdatedByLoginId = null,
                    Status = (int)messageBody.DocRegStatus,
                    ExpectedDateClose = messageBody.DocExpectCloseDateSpecified
                    ? messageBody.DocExpectCloseDate
                    : message.DocExpectCloseDate,
                    RejectReason = messageBody.RejectionReason
                };
                dbContext.SEOSStatusHistories.Add(record);
                dbContext.SaveChanges();

                return message;
            }
        }

        public static SEOSMessage GetMessageSender(Guid messageId, Guid eSubjectId)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                return dbContext.SEOSMessages
                    .Include(x => x.Receiver)
                    .Include(x => x.Sender)
                    .SingleOrDefault(x => x.MessageGuid == messageId && x.SenderElectronicSubjectId == eSubjectId);
            }
        }

        public static SEOSMessage GetMessageReceiver(Guid messageId, Guid eSubjectId)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                return dbContext.SEOSMessages
                        .Include(x => x.Sender)
                        .Include(x => x.Receiver)
                        .SingleOrDefault(x => x.MessageGuid == messageId && x.ReceiverElectronicSubjectId == eSubjectId);
            }
        }

        public static SEOSMessage UpdateDocumentStatus(int messageId, DocumentStatusType newStatus, DateTime? date, string rejectReason)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var message = dbContext.SEOSMessages.Find(messageId);

                if (message == null)
                    return null;

                message.DocExpectCloseDate = date;
                message.Status = (int)newStatus;
                message.DateUpdated = DateTime.Now;
                message.RejectedReason = newStatus == DocumentStatusType.DS_REJECTED
                    ? rejectReason
                    : null;

                dbContext.SaveChanges();

                var record = new SEOSStatusHistory()
                {
                    DateCreated = DateTime.Now,
                    MessageId = messageId,
                    UpdatedByLoginId = null,
                    Status = (int)newStatus,
                    ExpectedDateClose = date,
                    RejectReason = rejectReason

                };
                dbContext.SEOSStatusHistories.Add(record);
                dbContext.SaveChanges();

                return message;
            }
        }

        /// <summary>
        /// Get seos message by message guid
        /// </summary>
        /// <param name="guid">Message Guid</param>
        /// <param name="includeAllData">Include all data</param>
        /// <param name="includeAttachments">Include attachments</param>
        /// <returns>Message</returns>
        public static SEOSMessage GetMessage(Guid guid, bool includeAllData, bool includeAttachments)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var msg = dbContext.SEOSMessages.SingleOrDefault(x => x.MessageGuid == guid);
                if (msg == null)
                    return null;

                if (includeAllData)
                {
                    dbContext.Entry(msg).Reference(x => x.Sender).Load();
                    dbContext.Entry(msg).Reference(x => x.Receiver).Load();
                    dbContext.Entry(msg).Collection(x => x.Corespondents).Load();
                }

                if (includeAttachments)
                {
                    var attachments = dbContext.SEOSMessageAttachments
                        .Include(x => x.MalwareScanResult)
                        .Where(x => x.MessageId == msg.Id)
                        .Select(x => new
                        {
                            x.Comment,
                            x.Id,
                            x.Name,
                            x.MalwareScanResult
                        }).ToList();

                    msg.Attachments = attachments
                        .Select(x => new SEOSMessageAttachment()
                        {
                            Comment = x.Comment,
                            Id = x.Id,
                            Name = x.Name,
                            MalwareScanResult = x.MalwareScanResult
                        }).ToList();
                }

                return msg;
            }
        }

        public static SEOSMessage GetReceivedMessageByDocId(Guid docGuid, int recipientId)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var msg = dbContext.SEOSMessages
                    .SingleOrDefault(x => x.DocGuid == docGuid && x.ReceiverEntityId == recipientId);

                if (msg != null)
                {
                    dbContext.Entry(msg).Reference(x => x.Sender).Load();
                    dbContext.Entry(msg).Reference(x => x.Receiver).Load();
                    dbContext.Entry(msg).Collection(x => x.Corespondents).Load();
                }

                return msg;
            }
        }

        /// <summary>
        /// Get document attachment
        /// </summary>
        /// <param name="messageGuid">message Guid</param>
        /// <param name="attachmentId">seosmessageattachment id</param>
        /// <returns></returns>
        public static SEOSMessageAttachment GetDocumentAttachment(Guid? messageGuid, int attachmentId)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                if (messageGuid.HasValue)
                    return dbContext.SEOSMessageAttachments
                        .Include(x => x.Message)
                        .FirstOrDefault(x => x.Id == attachmentId && x.Message.MessageGuid == messageGuid);
                else
                    return
                        dbContext.SEOSMessageAttachments
                        .FirstOrDefault(x => x.Id == attachmentId);
            }
        }

        public static async Task<int> CreateSEOSDocumentAttachmetn(AttachmentRequest document)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var tempFile = dbContext.SEOSMessageAttachments.Create();
                tempFile.Name = document.Name;
                tempFile.Content = document.Content;
                tempFile.MimeType = document.MimeType;
                dbContext.SEOSMessageAttachments.Add(tempFile);
                await dbContext.SaveChangesAsync();

                return tempFile.Id;
            }
        }

        public static async Task<MalwareScanResult> UpdateSEOSDocumentMalwareScanResult(int documentId, MalwareScanSettings malwareScanResult)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var file = dbContext.SEOSMessageAttachments
                    .Include(x => x.MalwareScanResult)
                    .SingleOrDefault(x => x.Id == documentId);

                if (file.MalwareScanResult == null)
                {
                    file.MalwareScanResult = dbContext.MalwareScanResults.Create();
                }

                file.MalwareScanResult.MalwareId = malwareScanResult.MalwareId;
                file.MalwareScanResult.Status = (byte)malwareScanResult.Status;
                file.MalwareScanResult.IsMalicious = malwareScanResult.IsMalicious;
                file.MalwareScanResult.StatusDate = malwareScanResult.StatusDate;
                file.MalwareScanResult.Message = malwareScanResult.Message;

                await dbContext.SaveChangesAsync();
                return file.MalwareScanResult;
            }
        }

        public static SEOSMessage OpenReceivedDocumentAndRegister(int messageId, string currentLoginName, Guid currentLoginElesctronicSubjectId)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var message = dbContext.SEOSMessages.Single(x => x.Id == messageId);

                message.ReceiverLoginName = currentLoginName;
                message.ReceiverLoginElectronicSubjectId = currentLoginElesctronicSubjectId;
                message.Status = (int)DocumentStatusType.DS_REGISTERED;
                message.DateUpdated = DateTime.Now;
                message.DateRegistered = DateTime.Now;
                dbContext.SaveChanges();

                var record = new SEOSStatusHistory()
                {
                    DateCreated = DateTime.Now,
                    MessageId = messageId,
                    UpdatedByLoginId = null,
                    Status = (int)DocumentStatusType.DS_REGISTERED,
                    ExpectedDateClose = null,
                    RejectReason = null

                };
                dbContext.SEOSStatusHistories.Add(record);
                dbContext.SaveChanges();

                return message;
            }
        }

        public static SEOSMessage CreateSendMessage(
            MessageRequest request,
            List<SEOSMessageAttachment> attachments)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {

                var sender = DatabaseQueries.GetRegisteredEntityInEDelivery(request.ProfileIdentifier);
                if (sender == null)
                    throw new ApplicationException($"Sender with uic{request.ProfileIdentifier} not found");

                var receiver = DatabaseQueries.GetRegisteredEntity(request.ReceiverGuid);
                if (receiver == null)
                    throw new ApplicationException($"Receiver with Guid{request.ReceiverGuid} not found");

                var receiverESubjectId =
                    !DatabaseQueries.IsAS4Entity(receiver.EIK) && 
                    RegisteredEntitiesHelper.IsThroughEDelivery(receiver)
                    ? DatabaseQueries.GetElectronicSubjectIdByIdentifier(receiver.EIK)
                    : (Guid?)null;

                var corespondents = new List<SEOSMessageCorespondent>()
                {
                    new SEOSMessageCorespondent
                    {
                        Name = receiver.Name,
                        Phone = receiver.Phone,
                        Email = receiver.Email,
                        City = "Непосочен",
                        Kind = (int)CorrespondentKindType.Corr_Other
                    }
                };

                var message = new SEOSMessage()
                {
                    Attachments = attachments,
                    Corespondents = corespondents,
                    DateCreated = DateTime.Now,
                    DocAbout = request.Subject,
                    DocAttentionTo = request.DocumentAttentionTo,
                    DocComment = request.DocumentComment,
                    DocGuid = Guid.NewGuid(),
                    DocKind = request.DocumentKind,
                    DocReferenceNumber = request.ReferenceNumber,
                    DocReqDateClose = request.DocumentRequestCloseDate,
                    IsInitiatingDoc = true,
                    MessageDate = DateTime.Now,
                    MessageGuid = Guid.NewGuid(),
                    ReceiverElectronicSubjectId = receiverESubjectId,
                    ReceiverEntityId = receiver.Id,
                    SenderEntityId = sender.Id,
                    Status = (int)DocumentStatusType.DS_TRY_SEND,
                    SenderElectronicSubjectId = request.ProfileGuid,
                    SenderLoginName = request.LoginProfileName,
                    SenderLoginElectronicSubjectId = request.LoginProfileGuid
                };

                dbContext.SEOSMessages.Add(message);
                dbContext.SaveChanges();

                dbContext.Entry(message).Reference(x => x.Sender).Load();
                dbContext.Entry(message).Reference(x => x.Receiver).Load();
                return message;
            }
        }

        public static SEOSMessage CreateReceiveMessage(Message message, Guid receiverIdentifier, int? parMessageId)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            var jsSerializer = new JavaScriptSerializer();
            using (var dbContext = new SEOSDbContext(connectionString))
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                var messageBody = message.Body.Item as DocumentRegistrationRequestType;

                var numdate = message.GetDocNumberAndDate();
                var parNumdate = message.GetParDocNumberAndDate();
                var parDocGuid = message.GetParDocumentGuid();
                var sender = GetRegisteredEntity(Guid.Parse(message.Header.Sender.GUID));
                var receiver = GetRegisteredEntity(Guid.Parse(message.Header.Recipient.GUID));

                var dbMessage = new SEOSMessage()
                {
                    Status = (int)DocumentStatusType.DS_WAIT_REGISTRATION,
                    DocGuid = Guid.Parse(messageBody.Document.DocID.DocumentGUID),
                    DocNumberExternal = numdate.Item1,
                    DocDateExternal = numdate.Item2,
                    DateCreated = DateTime.Now,
                    DocAbout = messageBody.Document.DocAbout,
                    DocComment = messageBody.Document.DocComment,
                    DocKind = messageBody.Document.DocKind,
                    DocAttentionTo = messageBody.Document.DocAttentionTo,
                    DocReqDateClose = messageBody.Document.DocReqDateCloseSpecified
                    ? messageBody.Document.DocReqDateClose
                    : (DateTime?)null,
                    DocAddData = messageBody.Document.DocAddData != null
                    ? jsSerializer.Serialize(messageBody.Document.DocAddData)
                    : null,
                    MessageComment = messageBody.Comment,
                    MessageDate = message.Header.MessageDate,
                    MessageGuid = Guid.Parse(message.Header.MessageGUID),
                    ParentDocGuid = parDocGuid,
                    ParentDocNumber = parNumdate.Item1,
                    ReceiverElectronicSubjectId = receiverIdentifier,
                    SenderEntityId = sender.Id,
                    ReceiverEntityId = receiver.Id,
                    ServiceName = messageBody.Document.DocService?.ServiceName,
                    ServiceCode = messageBody.Document.DocService?.ServiceCode,
                    ServiceType = messageBody.Document.DocService?.ServiceType,
                    ParentMessageId = parMessageId,
                    IsInitiatingDoc = !parMessageId.HasValue
                };

                var attachments = messageBody.Document.DocAttachmentList;
                if (attachments != null)
                {
                    dbMessage.Attachments = MapperHelper.Mapping
                        .Map<ICollection<AttachmentFileType>, ICollection<SEOSMessageAttachment>>(attachments)
                        .ToArray();
                }

                var correspondents = messageBody.Document.DocCorrespondentList;
                if (correspondents != null)
                {
                    dbMessage.Corespondents = MapperHelper.Mapping
                        .Map<ICollection<CorrespondentType>, ICollection<SEOSMessageCorespondent>>(correspondents)
                        .ToArray();
                }

                dbContext.SEOSMessages.Add(dbMessage);
                dbContext.SaveChanges();
                dbContext.Entry(dbMessage).Reload();

                var record = new SEOSStatusHistory()
                {
                    DateCreated = DateTime.Now,
                    MessageId = dbMessage.Id,
                    UpdatedByLoginId = null,
                    Status = dbMessage.Status,
                    ExpectedDateClose = dbMessage.DocExpectCloseDate,
                    RejectReason = null

                };
                dbContext.SEOSStatusHistories.Add(record);
                dbContext.SaveChanges();
                transaction.Commit();

                return dbMessage;
            }
        }

        public static Dictionary<Guid, int> GetNewMessagesCount(List<Guid> profileGuidList)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            var guidList = profileGuidList
                .Select(q => (Guid?)q)
                .ToList();

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var result = dbContext.SEOSMessages
                    .Where(p => guidList.Contains(p.ReceiverElectronicSubjectId) &&
                      p.ReceiverLoginElectronicSubjectId == null &&
                      p.DateUpdated == null)
                    .GroupBy(q => q.ReceiverElectronicSubjectId)
                    .Select(r => new { profile = r.Key, count = r.Count() })
                    .ToDictionary(s => s.profile.Value, s => s.count);

                return result;
            }
        }

        public static void AddRetrySendRecord(
            int messageId,
            int receiverId,
            string messageBody)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var item = new SEOSRetrySendQueueItem
                {
                    MessageId = messageId,
                    MessageBody = messageBody,
                    DateFirstSent = DateTime.Now,
                    DateLastRetry = DateTime.Now.AddMinutes(15),
                    RetryCount = 0,
                    ReceiverId = receiverId
                };
                dbContext.SEOSRetrySendQueue.Add(item);
                dbContext.SaveChanges();
            }
        }

        public static List<SendMessageProperties> GetRetrySendRecords()
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var result =
                    (from rsq in dbContext.SEOSRetrySendQueue
                     join m in dbContext.SEOSMessages on rsq.MessageId equals m.Id
                     join s in dbContext.RegisteredEntities on m.SenderEntityId equals s.Id
                     join r in dbContext.RegisteredEntities on m.ReceiverEntityId equals r.Id
                     where rsq.DateLastRetry <= DateTime.Now
                     select new
                     {
                         Id = m.Id,
                         MessageGuid = m.MessageGuid,
                         DocGuid = m.DocGuid,
                         DocNumberInternal = m.DocNumberInternal,
                         DateCreated = m.DateCreated,
                         ReceiverId = m.Receiver.Id,
                         ReceiverServiceUrl = r.ServiceUrl,
                         ReceiverAdministrativeBodyName = r.Name,
                         ReceiverGUID = r.UniqueId,
                         ReceiverIdentifier = r.EIK,
                         SenderCertificateSN = s.CertificateSN,
                         SenderAdministrativeBodyName = s.Name,
                         SenderGUID = s.UniqueId,
                         SenderIdentifier = s.EIK,
                         MessageXml = rsq.MessageBody
                     })
                     .ToList();

                return result.Select( p => new SendMessageProperties
                {
                    Id = p.Id,
                    MessageGuid = p.MessageGuid,
                    DocIdentity = new DocumentIdentificationType
                    {
                        DocumentGUID = p.DocGuid.ToString("B"),
                        Item = new DocumentNumberType
                        {
                            DocNumber = p.DocNumberInternal,
                            DocDate = p.DateCreated
                        }
                    },
                    ReceiverId = p.ReceiverId,
                    ReceiverServiceUrl = p.ReceiverServiceUrl,
                    Receiver = new EntityNodeType
                    {
                        AdministrativeBodyName = p.ReceiverAdministrativeBodyName,
                        GUID = p.ReceiverGUID.ToString("B"),
                        Identifier = p.ReceiverIdentifier
                    },
                    SenderCertificateSN = p.SenderCertificateSN,
                    Sender = new EntityNodeType
                    {
                        AdministrativeBodyName = p.SenderAdministrativeBodyName,
                        GUID = p.SenderGUID.ToString("B"),
                        Identifier = p.SenderIdentifier
                    },
                    MessageXml = p.MessageXml
                }).ToList();
            }
        }

        public static void UpdateRetrySendRecords(
            List<(int Id, SubmitStatusRequestResult Request)> retryResults,
            int maxRetryCount)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                foreach (var result in retryResults)
                {
                    var message = dbContext.SEOSMessages
                        .SingleOrDefault(x => x.Id == result.Id);

                    var retrySettings = dbContext.SEOSRetrySendQueue
                        .SingleOrDefault(x => x.MessageId == result.Id);

                    if (message == null || retrySettings == null)
                        continue;

                    switch (result.Request.Status)
                    {
                        case DocumentStatusType.DS_TRY_SEND:
                            if (retrySettings.RetryCount + 1 == maxRetryCount)
                            {
                                message.Status = (int)DocumentStatusType.DS_SENT_FAILED;
                                dbContext.SEOSRetrySendQueue.Remove(retrySettings);
                            }
                            else
                            {
                                /*  Опит   Интервал   Време след първия опит
                                    1.     –          –
                                    2.     15 мин.    15 мин.
                                    3.     30 мин.    45 мин.
                                    4.     1 ч.       1 ч. и 45 мин.
                                    5.     2 ч.       3 ч. и 45 мин.
                                    6.     4 ч.       7 ч. и 45 мин.
                                    7.     8 ч.       15 ч. и 45 мин.
                                    8.     16 ч.      1 ден, 7 ч. и 45 мин.
                                    9.     32 ч.      2 дни, 15 ч. и 45 мин.
                                    10.    64 ч.      5 дни, 7 ч. и 45 мин.
                                    11.    128 ч.     10 дни, 15 ч. и 45 мин. */

                                retrySettings.RetryCount++;
                                retrySettings.DateLastRetry = retrySettings.DateLastRetry
                                    .Value
                                    .AddMinutes(15 * (1 << retrySettings.RetryCount));
                            }
                            break;
                        case DocumentStatusType.DS_SENT_FAILED:
                            message.Status = (int)DocumentStatusType.DS_SENT_FAILED;
                            dbContext.SEOSRetrySendQueue.Remove(retrySettings);
                            break;
                        default:
                            DatabaseQueries.ApplySentDocumentStatus(
                                message.Id, 
                                result.Request);
                            dbContext.SEOSRetrySendQueue.Remove(retrySettings);
                            break;
                    }

                    dbContext.SaveChanges();
                }
            }
        }

        public static void LogCommunication(
            Guid messageGuid,
            MessageType? messageType,
            string messageBody,
            bool isRequest,
            bool isInbound,
            string endpointUrl)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            var entity = GetEDeliveryEntity();
            var pattern = "<AttBody>[^<]*</AttBody>";
            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var log = new SEOSCommunicationLog
                {
                    MessageGuid = messageGuid,
                    MessageBody = Regex.Replace(messageBody ?? String.Empty, pattern, string.Empty),
                    MessageType = messageType?.ToString(),
                    DateCreated = DateTime.Now,
                    IsRequest = isRequest,
                    IsInbound = isInbound,
                    ReceiverUrl = endpointUrl ?? (isRequest
                    ? entity.ServiceUrl
                    : null)
                };

                dbContext.SEOSCommunicationLogs.Add(log);
                dbContext.SaveChanges();
            }
        }

        public static void SaveAs4TransferLog(Message message, string guid, TransferTypeEnum transferType)
        {
            if (message == null)
                return;
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var attachedDocs = GetAttachedDocs(message);

                var item = new AS4MessagesTransferLog
                {
                    MessageGUID = guid,
                    MessageType = message.Header.MessageType.ToString(),
                    TransferDate = DateTime.Now,
                    TransferType = (short)transferType,
                    SenderName = message.Header.Sender.AdministrativeBodyName,
                    SenderIdentifier = message.Header.Sender.Identifier,
                    ReceiverName = message.Header.Recipient.AdministrativeBodyName,
                    ReceiverIdentifier = message.Header.Recipient.Identifier,
                    AttachedFilesCount = attachedDocs.Count,
                    AttachedFilesSize = attachedDocs.Select(p => p.AttBody.Length).Sum()
                };

                dbContext.AS4MessagesTransferLog.Add(item);
                dbContext.SaveChanges();
            }
        }

        public static void SaveAs4SentMessage(Message message, string guid)
        {
            if (message == null)
                return;
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var item = new AS4SentMessagesStatus
                {
                    AS4Guid = guid,
                    MessageGuid = message.Header.MessageGUID,
                    MessageType = message.Header.MessageType.ToString(),
                    DocGuid = message.GetDocumentGuid().ToString(),
                    SenderIdentifier = message.Header.Sender.Identifier,
                    ReceiverIdentifier = message.Header.Recipient.Identifier,
                    Status = (int)messageStatus.READY_TO_SEND,
                    DateSent = DateTime.Now,
                    DateStatusChange = DateTime.Now,
                    SentResponse = false
                };

                dbContext.AS4SentMessagesStatus.Add(item);
                dbContext.SaveChanges();
            }
        }

        public static List<AS4SentMessagesStatus> GetAs4SentMessageStatus()
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            var finishedStatus = new List<messageStatus>
                {
                    messageStatus.ACKNOWLEDGED,
                    messageStatus.ACKNOWLEDGED_WITH_WARNING,
                    messageStatus.NOT_FOUND,
                    messageStatus.RECEIVED,
                    messageStatus.RECEIVED_WITH_WARNINGS,
                    messageStatus.DELETED,
                    messageStatus.DOWNLOADED
                };

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                return dbContext.AS4SentMessagesStatus
                    .Where(p => !finishedStatus.Contains((messageStatus)p.Status) && !p.SentResponse)
                    .OrderBy(q => q.Id)
                    .ToList();
            }
        }

        public static void UpdateAs4SentMessageStatus(List<AS4SentMessagesStatus> list)
        {
            if (list.Count == 0)
                return;

            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;
            var ids = list.ToDictionary(q => q.Id, q => q);

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var messages = dbContext.AS4SentMessagesStatus
                    .Where(p => ids.Keys.Contains(p.Id));

                foreach (var msg in messages)
                {
                    msg.Status = ids[msg.Id].Status;
                    msg.SentResponse = ids[msg.Id].SentResponse;
                    msg.DateStatusChange = ids[msg.Id].DateStatusChange;
                }
                dbContext.SaveChanges();
            }
        }

        private static List<AttachmentFileType> GetAttachedDocs(Message message)
        {
            if (message == null || !(message.Body.Item is DocumentRegistrationRequestType))
                return new List<AttachmentFileType>();

            var doc = (message.Body.Item as DocumentRegistrationRequestType).Document;
            if (doc == null || doc.DocAttachmentList == null)
                return new List<AttachmentFileType>();

            return doc.DocAttachmentList.ToList();
        }

        public static bool HasAS4Node(string uniqueId)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                return dbContext.AS4RegisteredEntities.Any(p => p.EIK == uniqueId);
            }
        }

        public static string GetAS4Node(string uniqueId)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var node = dbContext.AS4RegisteredEntities
                    .FirstOrDefault(p => p.EIK == uniqueId);

                return node != null
                    ? node.AS4Node
                    : String.Empty;
            }
        }

        public static bool IsAS4Entity(string uic)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliverySEOSDB"].ConnectionString;
            var eDeliveryGuid = WebConfigurationManager.AppSettings["SEOS.EDeliveryGUID"];

            using (var dbContext = new SEOSDbContext(connectionString))
            {
                var as4Entity = dbContext.AS4RegisteredEntities
                    .FirstOrDefault(p => p.EIK == uic);

                if (as4Entity == null)
                    return false;

                return String.Compare(eDeliveryGuid.Trim(), as4Entity.AS4Node.Trim(), true) != 0;
            }
        }

        /// <summary>
        /// Get electronic subjectId of the receiver
        /// DocAttentionTo should be filled with the receiver EIK
        /// </summary>
        /// <param name="esubjectsDBConnString"></param>
        /// <param name="docAttentionTo"></param>
        /// <returns></returns>
        public static Guid? GetElectronicSubjectIdByIdentifier(string uniqueId)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliveryDB"].ConnectionString;

            using (var db = new EDeliveryDbContext(connectionString))
            {
                var query = new StringBuilder(
$@"SELECT [ElectronicSubjectId]
 FROM [Profiles] 
        INNER JOIN [TargetGroupProfiles] ON[Profiles].Id = [TargetGroupProfiles].ProfileId
 WHERE Identifier = @uniqueId AND IsActivated=1 AND TargetGroupId = @targetGroupId");

                var result = db
                    .Database
                    .SqlQuery<Guid?>(
                        query.ToString(),
                        new SqlParameter("@uniqueId", uniqueId),
                        new SqlParameter("@targetGroupId", (int)TargetGroupId.PublicAdministration))
                    .FirstOrDefault();

                return result;
            }
        }

        public static int GetTargetGroupId(string uniqueId)
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["EDeliveryDB"].ConnectionString;

            using (var db = new EDeliveryDbContext(connectionString))
            {
                var query = new StringBuilder(
$@"SELECT TargetGroupId
FROM [Profiles] 
    INNER JOIN [TargetGroupProfiles] ON[Profiles].Id = [TargetGroupProfiles].ProfileId
WHERE Identifier = @uniqueId AND IsActivated=1");

                var result = db
                    .Database
                    .SqlQuery<int>(query.ToString(), new SqlParameter("@uniqueId", uniqueId)).FirstOrDefault();

                return result;
            }
        }

        public static bool HasProfile(string uic)
        {
            var targetGroupId = GetTargetGroupId(uic);
            if (targetGroupId == (int)TargetGroupId.PublicAdministration)
                return true;
            return false;
        }
    }
}
