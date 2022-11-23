using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using ED.DomainServices.Profiles;

using EDelivery.WebPortal.Enums;
using EDelivery.WebPortal.Models;
using EDelivery.WebPortal.Models.SEOS;
using EDelivery.WebPortal.SeosService;
using EDelivery.WebPortal.Utils;
using EDelivery.WebPortal.Utils.Attributes;
using EDelivery.WebPortal.Utils.Cache;
using EDelivery.WebPortal.Utils.Filters;

namespace EDelivery.WebPortal.Controllers
{
    [SeosFilter]
    public class SEOSController : BaseController
    {
        private readonly Lazy<Profile.ProfileClient> profileClient;
        private readonly SEOSPostServiceClient seosClient;

        public SEOSController()
        {
            this.profileClient = new Lazy<Profile.ProfileClient>(
                () => Grpc.GrpcClientFactory.CreateProfileClient(), isThreadSafe: false);

            this.seosClient = new SEOSPostServiceClient();
        }

        /// <summary>
        /// Get received messages
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [BreadCrumb(2, typeof(EDeliveryResources.SEOS), "TitleSEOS", eLeftMenu.Seos)]
        public ActionResult Index()
        {
            SEOSIndexModel model = new SEOSIndexModel()
            {
                ProfileName = this.UserData.ActiveProfile.ProfileName,
            };

            return View(model);
        }

        /// <summary>
        /// Get received messages
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Help()
        {
            return File(
                "/Content/Documents/EDeliverySeosHelp.pdf",
                "application/pdf",
                "EDeliverySeosHelp.pdf");
        }

        /// <summary>
        /// Get received messages
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [BreadCrumb(3, typeof(EDeliveryResources.SEOS), "ReceivedDocumentsTitle", eLeftMenu.Seos)]
        public ActionResult ReceivedDocuments(
            int page = 1,
            eSortColumn sortColumn = eSortColumn.DateReceived,
            eSortOrder sortOrder = eSortOrder.Desc)
        {
            MessagesPageResponse pageDocuments =
                this.seosClient.GetReceivedDocuments(
                    this.UserData.ActiveProfile.ProfileGuid,
                    page,
                    SystemConstants.PageSize,
                    (SortColumnEnum)sortColumn,
                    (SortOrderEnum)sortOrder);

            List<SEOSDocumentModel> documents = pageDocuments
                .Messages
                .Select(x => new SEOSDocumentModel
                {
                    UniqueIdentifier = x.UniqueIdentifier,
                    Status = (DocumentStatusType)x.Status,
                    RegIndex = x.RegIndex,
                    Receiver = x.Receiver,
                    ReceiverId = x.ReceiverId,
                    Sender = x.Sender,
                    SenderId = x.SenderId,
                    Subject = x.Subject,
                    DateReceived = x.DateReceived,
                    DateSent = x.DateSent,
                    DocumentKind = x.DocumentKind,
                    DocumentReferenceNumber = x.DocumentReferenceNumber
                })
                .ToList();

            SEOSDocumentsPagedListModel model =
                new SEOSDocumentsPagedListModel()
                {
                    SortColumn = sortColumn,
                    SortOrder = sortOrder,
                    Documents = new PagedList.PagedListLight<SEOSDocumentModel>(
                        documents,
                        SystemConstants.PageSize,
                        page,
                        pageDocuments.CountAllMessages)
                };

            if (Request.IsAjaxRequest())
            {
                return PartialView("Partials/_ReceivedDocuments", model);
            }

            return View(model);
        }

        /// <summary>
        /// Get all sent messages
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [BreadCrumb(3, typeof(EDeliveryResources.SEOS), "SentDocumentsTitle", eLeftMenu.Seos)]
        public ActionResult SentDocuments(
            int page = 1,
            eSortColumn sortColumn = eSortColumn.DateSent,
            eSortOrder sortOrder = eSortOrder.Desc)
        {
            MessagesPageResponse pageDocuments =
                this.seosClient.GetSentDocuments(
                    this.UserData.ActiveProfile.ProfileGuid,
                    page,
                    SystemConstants.PageSize,
                    (SortColumnEnum)sortColumn,
                    (SortOrderEnum)sortOrder);

            List<SEOSDocumentModel> documents = pageDocuments
                .Messages
                .Select(x => new SEOSDocumentModel
                {
                    UniqueIdentifier = x.UniqueIdentifier,
                    Status = (DocumentStatusType)x.Status,
                    RegIndex = x.RegIndex,
                    Receiver = x.Receiver,
                    ReceiverId = x.ReceiverId,
                    Sender = x.Sender,
                    SenderId = x.SenderId,
                    Subject = x.Subject,
                    DateReceived = x.DateReceived,
                    DateSent = x.DateSent,
                    DocumentKind = x.DocumentKind,
                    DocumentReferenceNumber = x.DocumentReferenceNumber
                })
                .ToList();

            SEOSDocumentsPagedListModel model =
                new SEOSDocumentsPagedListModel()
                {
                    SortColumn = sortColumn,
                    SortOrder = sortOrder,
                    Documents = new PagedList.PagedListLight<SEOSDocumentModel>(
                        documents,
                        SystemConstants.PageSize,
                        page,
                        pageDocuments.CountAllMessages)
                };

            if (Request.IsAjaxRequest())
            {
                return PartialView("Partials/_SentDocuments", model);
            }

            return View(model);
        }

        /// <summary>
        /// Get registered entities
        /// </summary>
        /// <returns></returns>
        [BreadCrumb(3, typeof(EDeliveryResources.SEOS), "RegisteredEntities", eLeftMenu.Seos)]
        public ActionResult RegisteredEntities(
            string searchPhase = null,
            bool showOnlyActive = false)
        {
            RegisteredEntityResponse[] entities =
                this.seosClient.GetRegisteredEntities();

            if (!string.IsNullOrEmpty(searchPhase))
            {
                searchPhase = searchPhase.ToLower();
                entities = entities
                    .Where(x => x.AdministrationBodyName.ToLower().Contains(searchPhase) ||
                                x.EIK.ToLower().Contains(searchPhase))
                    .ToArray();
            }

            if (showOnlyActive)
            {
                entities = entities
                    .Where(x => x.Status == EntityServiceStatusEnum.Active)
                    .ToArray();
            }

            entities = entities.OrderBy(x => x.AdministrationBodyName).ToArray();

            List<SEOSEntityModel> modelEntities = entities
                .Select(x => new SEOSEntityModel
                {
                    UniqueIdentifier = x.UniqueIdentifier,
                    AdministrationBodyName = x.AdministrationBodyName,
                    Phone = x.Phone,
                    EIK = x.EIK,
                    Emailddress = x.Emailddress,
                    Status = x.Status,
                    ServiceUrl = x.ServiceUrl
                })
                .ToList();

            SEOSRegisteredEntitiesModel model =
                new SEOSRegisteredEntitiesModel()
                {
                    SearchPhase = searchPhase,
                    Entities = modelEntities,
                    ShowOnlyActive = showOnlyActive
                };

            RegisteredEntityResponse eDeliveryEntity =
                this.seosClient.GetEDeliveryEntity();

            ViewBag.EDeliveryGuid = eDeliveryEntity.UniqueIdentifier;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_RegisteredEntitiesList", model);
            }

            return View("RegisteredEntities", model);
        }

        /// <summary>
        /// View registered entities with option to send a message to chosen one
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [BreadCrumb(3, typeof(EDeliveryResources.SEOS), "SendDocumentToEntity", eLeftMenu.Seos)]
        public ActionResult RegisteredEntitiesSendDocument()
        {
            return RegisteredEntities(null, true);
        }

        /// <summary>
        /// Отваряне на изпратен документ
        /// </summary>
        /// <param name="messageGuid"></param>
        /// <returns></returns>
        [HttpGet]
        [BreadCrumb(4, typeof(EDeliveryResources.SEOS), "OpenSentDocument", eLeftMenu.Seos)]
        public ActionResult OpenSentDocument(
            [Bind(Prefix = "id")] Guid messageGuid)
        {
            SEOSOpenDocumentModel model = OpenDocument(
                messageGuid,
                this.UserData.ActiveProfile.ProfileGuid,
                false);

            if (model == null)
            {
                return RedirectToAction("Index", "Error", new { id = "403" });
            }

            return View(model);
        }

        /// <summary>
        /// Get seos entitiy information
        /// </summary>
        /// <param name="seosEntityId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SEOSEntitiyInfo(
            [Bind(Prefix = "id")] Guid seosEntityId)
        {
            SEOSEntityModel entityModel = null;

            RegisteredEntityResponse entity =
                this.seosClient.GetRegisteredEntity(seosEntityId);
            if (entity != null)
            {
                entityModel = new SEOSEntityModel
                {
                    UniqueIdentifier = entity.UniqueIdentifier,
                    AdministrationBodyName = entity.AdministrationBodyName,
                    Phone = entity.Phone,
                    EIK = entity.EIK,
                    Emailddress = entity.Emailddress,
                    Status = entity.Status,
                    ServiceUrl = entity.ServiceUrl
                };
            }

            return PartialView("Partials/_SEOSEntitiyInfo", entityModel);
        }

        /// <summary>
        /// Отваряне на получен документ
        /// </summary>
        /// <param name="messageGuid"></param>
        /// <returns></returns>
        [HttpGet]
        [BreadCrumb(4, typeof(EDeliveryResources.SEOS), "OpenReceivedDocument", eLeftMenu.Seos)]
        public ActionResult OpenReceivedDocument(
            [Bind(Prefix = "id")] Guid messageGuid)
        {
            SEOSOpenDocumentModel model = OpenDocument(
                messageGuid,
                this.UserData.ActiveProfile.ProfileGuid,
                true);

            if (model == null)
            {
                return RedirectToAction("Index", "Error", new { id = "403" });
            }

            return View(model);
        }

        /// <summary>
        /// Check document status
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckDocumentStatus(
            [Bind(Prefix = "id")] Guid messageId)
        {
            DocumentStatusType? documentStatus =
                this.seosClient.CheckDocumentStatus(
                    messageId,
                    this.UserData.ActiveProfile.ProfileGuid);

            string content = documentStatus == null
                ? string.Empty
                : SEOSHelper.GetSEOSStatusText(documentStatus.Value);

            return Content(content);
        }

        [HttpGet]
        public ActionResult UpdateDocumentStatus(
            [Bind(Prefix = "id")] Guid messageId)
        {
            ChangeDocumentStatusResponse status =
                this.seosClient.GetDocumentStatus(
                    messageId,
                    this.UserData.ActiveProfile.ProfileGuid);

            if (status == null)
            {
                return RedirectToAction("Index", "Error", new { id = "403" });
            }

            ChangeDocumentStatusModel model =
                new ChangeDocumentStatusModel
                {
                    MessageId = status.MessageId,
                    OldStatus = status.OldStatus,
                    Status = status.Status,
                    ExpectedDateClose = status.ExpectedDateClose,
                    RejectReason = status.RejectReason,
                };

            return View("_ChangeDocumentStatusPartial", "_LayoutPopup", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateDocumentStatus(
            ChangeDocumentStatusModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ChangeDocumentStatusRequest request = new ChangeDocumentStatusRequest
            {
                MessageId = model.MessageId,
                ProfileGuid = UserData.ActiveProfile.ProfileGuid,
                NewStatus = model.Status,
                ExpectedDateClose = model.ExpectedDateClose,
                RejectReason = model.RejectReason
            };

            bool result = this.seosClient.UpdateDocumentStatus(request);

            if (!result)
            {
                return RedirectToAction("Index", "Error", new { id = "403" });
            }

            return Json(new
            {
                Success = true,
                NewStatus = SEOSHelper.GetSEOSStatusText(model.Status)
            });
        }

        /// <summary>
        /// Download attachment
        /// </summary>
        /// <param name="messageGuid">messageGuid</param>
        /// <param name="attachmentId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DownloadAttachment(
            [Bind(Prefix = "id")] Guid messageGuid,
            [Bind(Prefix = "aid")] int attachmentId)
        {
            AttachmentResponse attachment =
                this.seosClient.GetDocumentAttachment(
                    messageGuid,
                    attachmentId);

            if (attachment == null)
            {
                throw new Exception(
                    string.Format(
                        EDeliveryResources.SEOS.ErrorNotFoundAttachment,
                        attachmentId));
            }

            Guid eSubjectId = this.UserData.ActiveProfile.ProfileGuid;

            if (attachment.ReceiverElectronicSubjectId != eSubjectId
                && attachment.SenderElectronicSubjectId != eSubjectId)
            {
                throw new UnauthorizedAccessException(
                    string.Format(
                        EDeliveryResources.SEOS.ErrorUnauthorizedAccesssToAttachment,
                        attachment.Name));
            }

            return File(
                attachment.Content,
                attachment.MimeType,
                attachment.Name);
        }

        /// <summary>
        /// View registered entities with option to send a message to chosen one
        /// </summary>
        /// <returns></returns>
        [BreadCrumb(3, typeof(EDeliveryResources.SEOS), "TitleCreateNewSEOSMessage", eLeftMenu.Seos)]
        public ActionResult NewSEOSDocument(
            [Bind(Prefix = "id")] Guid entityId)
        {
            RegisteredEntityResponse receiver =
                this.seosClient.GetRegisteredEntity(entityId);
            if (receiver == null)
            {
                return RedirectToAction("RegisteredEntitiesSendDocument");
            }

            SEOSDocumentDetailsModel seosDocumentModel =
                new SEOSDocumentDetailsModel()
                {
                    ReceiverGuid = receiver.UniqueIdentifier,
                    Receiver = receiver.AdministrationBodyName,
                    ElectronicSubjectId = UserData.ActiveProfile.ProfileGuid,
                    DocumenAttachments = new List<SEOSDocumentAttachmentModel>()
                    {
                        new SEOSDocumentAttachmentModel(),
                        new SEOSDocumentAttachmentModel(),
                        new SEOSDocumentAttachmentModel()
                    }
                };

            return View(seosDocumentModel);
        }

        [HttpPost]
        public JsonResult SubmitSEOSDocumentAttachment(
            HttpPostedFileBase seosattachment)
        {
            if (seosattachment == null)
            {
                return Json(new MalwareScanResultModel
                {
                    IsSuccessfulScan = false,
                    Message = EDeliveryResources.SEOS.ErrorPleaseSelectFile
                });
            }

            FileInfo fileinfo = new FileInfo(seosattachment.FileName);

            AttachmentRequest doc = new AttachmentRequest
            {
                Name = fileinfo.Name,
                Content = Utils.Utils.GetFileBytes(seosattachment),
                MimeType = seosattachment.ContentType,
                Comment = string.Empty
            };

            MalwareScanResultResponse scanResult =
                this.seosClient.SaveAttachmentWithScan(doc);

            MalwareScanResultModel result = new MalwareScanResultModel
            {
                DbItemId = scanResult.DbItemId,
                ErrorReason = scanResult.ErrorReason,
                FileName = scanResult.FileName,
                IsMalicious = scanResult.IsMalicious,
                IsSuccessfulScan = scanResult.IsSuccessfulScan,
                Message = scanResult.Message
            };

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitSEOSDocument(
            SEOSDocumentDetailsModel model)
        {
            if (model.ElectronicSubjectId != this.UserData.ActiveProfile.ProfileGuid)
            {
                return RedirectToAction("Index", "Profile");
            }

            if (model.DocumenAttachmentFirstContent == 0)
            {
                ModelState.AddModelError(
                    nameof(model.DocumenAttachmentFirstContent),
                    EDeliveryResources.SEOS.ErrorDocumenntRequired);
            }

            if (!ModelState.IsValid)
            {
                return View("NewSEOSDocument", model);
            }

            try
            {
                GetProfileResponse response =
                    await this.profileClient.Value.GetProfileAsync(
                        new GetProfileRequest
                        {
                            ProfileId = UserData.ActiveProfile.ProfileId
                        },
                        cancellationToken: Response.ClientDisconnectedToken);

                CachedLoginProfile currentLogin = UserData
                    .Profiles
                    .Single(x => x.IsDefault);

                CorespondentRequest corespondent = new CorespondentRequest
                {
                    Address = response.Residence,
                    Bulstat = response.Identifier,
                    City = !string.IsNullOrEmpty(response.City)
                        ? response.City
                        : "Непосочен",
                    Email = response.Email,
                    MobilePhone = response.Phone,
                    Name = response.ProfileName
                };

                MessageRequest request = new MessageRequest
                {
                    ElectronicSubjectId = model.ElectronicSubjectId,
                    ReceiverGuid = model.ReceiverGuid,
                    Receiver = model.Receiver,
                    Subject = model.Subject,
                    DocumentKind = model.DocumentKind,
                    ReferenceNumber = model.ReferenceNumber,
                    DocumentAttentionTo = model.DocumentAttentionTo,
                    DocumentComment = model.DocumentComment,
                    DocumentRequestCloseDate = model.DocumentRequestCloseDate,
                    DocumenAttachmentFirstContent = model.DocumenAttachmentFirstContent,
                    DocumenAttachmentFirstFileName = model.DocumenAttachmentFirstFileName,
                    DocumenAttachmentFirstComment = model.DocumenAttachmentFirstComment,
                    ProfileIdentifier = UserData.ActiveProfile.Identifier,
                    ProfileGuid = UserData.ActiveProfile.ProfileGuid,
                    LoginProfileName = currentLogin.ProfileName,
                    LoginProfileGuid = currentLogin.ProfileGuid,
                    DocumenAttachments = model.DocumenAttachments
                        .Select(x => new AttachmentRequest
                        {
                            Id = x.TempId,
                            Comment = x.Comment,
                            Name = x.FileName

                        })
                        .ToArray()
                };

                SubmitStatusRequestResult result =
                    this.seosClient.SendMessage(request, corespondent);

                return RedirectToAction("SentDocuments");
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    "Error submiting a new document");

                // TODO: there is no validation summary
                ModelState.AddModelError(
                    "",
                    EDeliveryResources.SEOS.ErrorSubmitNewDocument);

                return View("NewSeosDocument", model);
            }
        }

        private SEOSOpenDocumentModel OpenDocument(
            Guid messageGuid,
            Guid electronicSubjectId,
            bool isReceived)
        {
            // TODO: why is it working with default profile
            CachedLoginProfile currentLogin =
                UserData.Profiles.Single(x => x.IsDefault);

            OpenDocumentRequest request = new OpenDocumentRequest
            {
                MessageGuid = messageGuid,
                ESubjectId = electronicSubjectId,
                ProfileGuid = currentLogin.ProfileGuid,
                ProfileName = currentLogin.ProfileName,
                IsReceived = isReceived
            };

            OpenDocumentResponse doc = this.seosClient.OpenDocument(request);
            if (doc == null)
            {
                return null;
            }

            SEOSOpenDocumentModel model = new SEOSOpenDocumentModel
            {
                MessageGuid = doc.MessageGuid,
                IsReceived = doc.IsReceived,
                Attachments = doc.Attachments.ToList(),
                Corespondents = doc.Corespondents.ToList(),
                DateReceived = doc.DateReceived,
                SenderName = doc.SenderName,
                SenderGuid = doc.SenderGuid,
                ReceiverLoginName = doc.ReceiverLoginName,
                DateRegistered = doc.DateRegistered,
                DateSent = doc.DateSent,
                ReceiverName = doc.ReceiverName,
                DocReferenceNumber = doc.DocReferenceNumber,
                ReceiverGuid = doc.ReceiverGuid,
                SenderLoginName = doc.SenderLoginName,
                RejectedReason = doc.RejectedReason,
                AttentionTo = doc.AttentionTo,
                RequestedCloseDate = doc.RequestedCloseDate,
                LastStatusChangeDate = doc.LastStatusChangeDate,
                Subject = doc.Subject,
                Comment = doc.Comment,
                DocAdditionalData = doc.DocAdditionalData,
                InternalRegIndex = doc.InternalRegIndex,
                ExternalRegIndex = doc.ExternalRegIndex,
                DocGuid = doc.DocGuid,
                DocKind = doc.DocKind,
                Status = (DocumentStatusType)doc.Status,
                Service = new SEOSSerivceModel
                {
                    ServiceName = doc.Service.ServiceName,
                    ServiceCode = doc.Service.ServiceCode,
                    ServiceType = doc.Service.ServiceType
                }
            };

            return model;
        }
    }
}
