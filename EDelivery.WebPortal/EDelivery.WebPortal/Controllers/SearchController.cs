using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

using ED.DomainServices.Messages;

using EDelivery.WebPortal.Authorization;
using EDelivery.WebPortal.Enums;
using EDelivery.WebPortal.Grpc;
using EDelivery.WebPortal.Models;

using EDeliveryResources;

namespace EDelivery.WebPortal.Controllers
{
    [EDeliveryResourceAuthorize]
    public class SearchController : BaseController
    {
        private readonly Lazy<Message.MessageClient> messageClient;

        public SearchController()
        {
            this.messageClient =
                new Lazy<Message.MessageClient>(
                    () => GrpcClientFactory.CreateMessageClient(), isThreadSafe: false);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.SearchMessageRecipients)]
        [HttpGet]
        public async Task<ActionResult> Search(int templateId)
        {
            GetTargetGroupsFromMatrixResponse targetGroups =
                await this.messageClient.Value.GetTargetGroupsFromMatrixAsync(
                    new GetTargetGroupsFromMatrixRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            SearchModel model = new SearchModel(templateId)
            {
                CanSendToIndividuals = targetGroups
                    .Result
                    .Any(e => e.TargetGroupId == (int)TargetGroupId.Individual),
                CanSendToLegalEntities = targetGroups
                    .Result
                    .Any(e => e.TargetGroupId == (int)TargetGroupId.LegalEntity),
                TargetGroups = new SelectListItem[]
                {
                    new SelectListItem
                    {
                        Text = Common.OptionAll,
                        Value = null,
                        Selected = true
                    }
                }
                .Concat(
                    targetGroups.Result
                        .Where(e => e.TargetGroupId != (int)TargetGroupId.Individual
                            && e.TargetGroupId != (int)TargetGroupId.LegalEntity)
                        .Select(e => new SelectListItem
                        {
                            Text = e.Name,
                            Value = e.TargetGroupId.ToString()
                        }))
                .ToList(),
            };

            return PartialView("Search", model);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.SearchMessageRecipientIndividuals)]
        [HttpPost]
        public async Task<ActionResult> SearchIndividual(SearchIndividualModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("Partials/_SearchIndividual", model);
            }

            FindRecipientIndividualResponse individual =
                await this.messageClient.Value.FindRecipientIndividualAsync(
                    new FindRecipientIndividualRequest
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Identifier = model.Identifier,
                        TemplateId = model.TemplateId,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            if (individual.Individual == null)
            {
                ModelState.AddModelError(
                    nameof(model.FirstName),
                    EDeliveryResources.ErrorMessages.ErrorPersonNotFound);

                return PartialView("Partials/_SearchIndividual", model);
            }

            ModelState.Clear();

            SearchIndividualModel vm = new SearchIndividualModel
            {
                SelectedIndividualProfileId = individual.Individual.ProfileId,
                SelectedIndividualProfileName = individual.Individual.Name
            };

            return PartialView("Partials/_SearchIndividual", vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.SearchMessageRecipientLegalEntities)]
        [HttpPost]
        public async Task<ActionResult> SearchLegalEntity(SearchLegalEntityModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("Partials/_SearchLegalEntity", model);
            }

            FindRecipientLegalEntityResponse legalEntity =
                await this.messageClient.Value.FindRecipientLegalEntityAsync(
                    new FindRecipientLegalEntityRequest
                    {
                        Identifier = model.CompanyRegistrationNumber,
                        TemplateId = model.TemplateId,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            if (legalEntity.LegalEntity == null)
            {
                ModelState.AddModelError(
                    nameof(model.CompanyRegistrationNumber),
                    EDeliveryResources.ErrorMessages.ErrorLegalNotFound);

                return PartialView("Partials/_SearchLegalEntity", model);
            }

            ModelState.Clear();

            SearchLegalEntityModel vm = new SearchLegalEntityModel
            {
                SelectedLegalEntityProfileId = legalEntity.LegalEntity.ProfileId,
                SelectedLegalEntityProfileName = legalEntity.LegalEntity.Name
            };

            return PartialView("Partials/_SearchLegalEntity", vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.SearchMessageRecipients)]
        [HttpGet]
        public async Task<JsonResult> QueryRecipients(
            string term,
            int templateId,
            int? targetGroupId,
            int? page)
        {
            page = page ?? 1;
            int pageSize = SystemConstants.Select2PageSize;

            FindRecipientProfilesResponse profiles =
                await this.messageClient.Value.FindRecipientProfilesAsync(
                    new FindRecipientProfilesRequest
                    {
                        Term = term ?? string.Empty,
                        TargetGroupId = targetGroupId,
                        TemplateId = templateId,
                        Offset = (page.Value - 1) * pageSize,
                        // take 1 more so that we know there are more
                        Limit = pageSize + 1,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            var result = profiles
                .Result
                .Select(e => new { id = e.ProfileId, text = e.Name })
                .Take(pageSize)
                .ToList();

            return Json(
                new
                {
                    results = result,
                    pagination = new { more = profiles.Result.Count > pageSize }
                },
                JsonRequestBehavior.AllowGet);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.SearchMessageRecipients)]
        [HttpGet]
        public async Task<JsonResult> QueryRecipientGroups(
            string term,
            int templateId,
            int? page)
        {
            page = page ?? 1;
            int pageSize = SystemConstants.Select2PageSize;

            GetRecipientGroupsResponse recipientGroups =
               await this.messageClient.Value.GetRecipientGroupsAsync(
                   new GetRecipientGroupsRequest
                   {
                       Term = term,
                       ProfileId = this.UserData.ActiveProfileId,
                       TemplateId = templateId,
                       Offset = (page.Value - 1) * pageSize,
                       // take 1 more so that we know there are more
                       Limit = pageSize + 1,
                   },
                   cancellationToken: Response.ClientDisconnectedToken);

            var result = recipientGroups
                .Result
                .Select(e => new { id = e.RecipientGroupId, text = e.Name })
                .Take(pageSize)
                .ToList();

            return Json(
                new
                {
                    results = result,
                    pagination = new { more = recipientGroups.Result.Count > pageSize }
                },
                JsonRequestBehavior.AllowGet);
        }

        // TODO: rename and remake
        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.ForwardMessage,
            MessageIdRouteOrQueryParam = "messageId")]
        [HttpGet]
        public async Task<JsonResult> GetInstitutions(
            string term,
            int? page)
        {
            page = page ?? 1;
            int pageSize = SystemConstants.Select2PageSize;

            GetInstitutionsResponse institutions =
                await this.messageClient.Value.GetInstitutionsAsync(
                    new GetInstitutionsRequest
                    {
                        Term = term ?? string.Empty,
                        Offset = (page.Value - 1) * pageSize,
                        Limit = pageSize + 1
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            var result = institutions
                .Institutions
                .Select(e => new
                {
                    text = e.Name,
                    id = e.ProfileId.ToString()
                })
                .Take(pageSize)
                .ToList();

            return Json(
                new
                {
                    results = result,
                    pagination = new { more = institutions.Institutions.Count > pageSize }
                },
                JsonRequestBehavior.AllowGet);
        }
    }
}
