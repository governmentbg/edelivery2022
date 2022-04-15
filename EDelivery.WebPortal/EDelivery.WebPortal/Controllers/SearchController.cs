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
        public async Task<ActionResult> Search()
        {
            SearchModel model = await GetSearchModel();

            return PartialView("Search", model);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.SearchMessageRecipientIndividuals)]
        [HttpPost]
        public async Task<ActionResult> SearchPerson(SearchPersonModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("Partials/_SearchPerson", model);
            }

            FindIndividualResponse individual =
                await this.messageClient.Value.FindIndividualAsync(
                    new FindIndividualRequest
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Identifier = model.Identifier
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            if (individual.Individual == null)
            {
                ElmahLogger.Instance.Info(
                    "Person with EGN: {0}, FirstName: {1} and LastName: {2} is not found in the database!",
                    model.Identifier,
                    model.FirstName,
                    model.LastName);

                ModelState.AddModelError(
                    nameof(model.FirstName),
                    EDeliveryResources.ErrorMessages.ErrorPersonNotFound);

                return PartialView("Partials/_SearchPerson", model);
            }

            ModelState.Clear();

            SearchPersonModel vm = new SearchPersonModel
            {
                SelectedIndividualProfileId = individual.Individual.ProfileId,
                SelectedIndividualProfileName = individual.Individual.Name
            };

            return PartialView("Partials/_SearchPerson", vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.SearchMessageRecipientLegalEntities)]
        [HttpPost]
        public async Task<ActionResult> SearchLegal(SearchLegalModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("Partials/_SearchLegal");
            }

            FindLegalEntityResponse legalEntity =
                await this.messageClient.Value.FindLegalEntityAsync(
                    new FindLegalEntityRequest
                    {
                        Identifier = model.CompanyRegistrationNumber
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            if (legalEntity.LegalEntity == null)
            {
                ElmahLogger.Instance.Info(
                    "Legal Person with EIK: {0}",
                    model.CompanyRegistrationNumber);

                ModelState.AddModelError(
                    nameof(model.CompanyRegistrationNumber),
                    EDeliveryResources.ErrorMessages.ErrorLegalNotFound);

                return PartialView("Partials/_SearchLegal");
            }

            ModelState.Clear();

            SearchLegalModel vm = new SearchLegalModel
            {
                SelectedLegalEntityProfileId = legalEntity.LegalEntity.ProfileId,
                SelectedLegalEntityProfileName = legalEntity.LegalEntity.Name
            };

            return PartialView("Partials/_SearchLegal", vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.SearchMessageRecipients)]
        [HttpGet]
        public async Task<JsonResult> QueryRecipients(
            string term,
            int? targetGroupId,
            int? page)
        {
            page = page ?? 1;
            int pageSize = SystemConstants.Select2PageSize;

            FindProfilesResponse profiles =
                await this.messageClient.Value.FindProfilesAsync(
                    new FindProfilesRequest
                    {
                        Term = term ?? string.Empty,
                        TargetGroupId = targetGroupId,
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

        private async Task<SearchModel> GetSearchModel()
        {
            GetRecipientGroupsResponse recipientGroups =
                await this.messageClient.Value.GetRecipientGroupsAsync(
                    new GetRecipientGroupsRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            GetTargetGroupsResponse targetGroups =
                await this.messageClient.Value.GetTargetGroupsAsync(
                    new GetTargetGroupsRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            SearchModel model = new SearchModel()
            {
                CanSendToIndividuals = targetGroups
                    .Result
                    .Any(e => e.TargetGroupId == (int)TargetGroupId.Individual),
                CanSendToLegalEntities = targetGroups
                    .Result
                    .Any(e => e.TargetGroupId == (int)TargetGroupId.LegalEntity),
                RecipientGroups = recipientGroups
                    .Result
                    .Select(e => new SelectListItem
                    {
                        Text = e.Name,
                        Value = e.RecipientGroupId.ToString()
                    })
                    .ToList(),
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

            return model;
        }
    }
}
