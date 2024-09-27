using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ED.AdminPanel.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class NomenclaturesController
    {
        private const int PageSize = 30;

#pragma warning disable IDE1006 // Naming Styles
        public record Select2Result(
            Select2ResultItem[] results,
            Select2ResultPagination pagination);

        public record Select2ResultItem(
            string id,
            string text);

        public record Select2ResultPagination(bool more);
#pragma warning restore IDE1006 // Naming Styles

        public async Task<ActionResult<object>> ProfilesAsync(
            [FromQuery] string term,
            [FromQuery(Name = "page")] int? queryPage,
            [FromQuery] int[] ids,
            [FromServices] Admin.AdminClient adminClient,
            CancellationToken ct)
        {
            if (ids != null && ids.Length > 0)
            {
                GetProfilesByIdResponse profilesByIdResponse =
                    await adminClient.GetProfilesByIdAsync(
                        new GetProfilesByIdRequest
                        {
                            Ids = { ids }
                        },
                        cancellationToken: ct);

                return profilesByIdResponse.Items
                    .Select(n =>
                        new Select2ResultItem(
                            n.ProfileId.ToString(),
                            n.ProfileName))
                    .ToArray();
            }

            int page;
            if (queryPage != null)
            {
                page = queryPage.Value;
            }
            else
            {
                page = 1;
            }

            ListProfilesResponse profilesResponse =
                await adminClient.ListProfilesAsync(
                    new ListProfilesRequest
                    {
                        Term = term ?? string.Empty,
                        Offset = (page - 1) * PageSize,
                        // take 1 more so that we know there are more
                        Limit = PageSize + 1,
                    },
                    cancellationToken: ct);

            return new Select2Result(
                profilesResponse.Items
                    .Select(n =>
                        new Select2ResultItem(
                            n.ProfileId.ToString(),
                            n.ProfileName))
                    .ToArray(),
                new Select2ResultPagination(
                    profilesResponse.Items.Count > PageSize)
            );
        }

        public async Task<ActionResult<object>> RecipientsAsync(
            [FromQuery] string term,
            [FromQuery(Name = "page")] int? queryPage,
            [FromQuery] int[] ids,
            [FromServices] Admin.AdminClient adminClient,
            CancellationToken ct)
        {
            if (ids != null && ids.Length > 0)
            {
                GetProfilesByIdResponse profilesByIdResponse =
                    await adminClient.GetProfilesByIdAsync(
                        new GetProfilesByIdRequest
                        {
                            Ids = { ids }
                        },
                        cancellationToken: ct);

                return profilesByIdResponse.Items
                    .Select(n =>
                        new Select2ResultItem(
                            n.ProfileId.ToString(),
                            n.ProfileName))
                    .ToArray();
            }

            int page;
            if (queryPage != null)
            {
                page = queryPage.Value;
            }
            else
            {
                page = 1;
            }

            ListRecipientsResponse profilesResponse =
                await adminClient.ListRecipientsAsync(
                    new ListRecipientsRequest
                    {
                        Term = term ?? string.Empty,
                        Offset = (page - 1) * PageSize,
                        // take 1 more so that we know there are more
                        Limit = PageSize + 1,
                    },
                    cancellationToken: ct);

            return new Select2Result(
                profilesResponse.Items
                    .Select(n =>
                        new Select2ResultItem(
                            n.ProfileId.ToString(),
                            n.ProfileName))
                    .ToArray(),
                new Select2ResultPagination(
                    profilesResponse.Items.Count > PageSize)
            );
        }

        public async Task<ActionResult<object>> TargetGroupsAsync(
            [FromQuery] string term,
            [FromQuery(Name = "page")] int? queryPage,
            [FromQuery] int[] ids,
            [FromServices] Admin.AdminClient adminClient,
            CancellationToken ct)
        {
            if (ids != null && ids.Length > 0)
            {
                GetTargetGroupsByIdResponse targetGroupsByIdResponse =
                    await adminClient.GetTargetGroupsByIdAsync(
                        new GetTargetGroupsByIdRequest
                        {
                            Ids = { ids }
                        },
                        cancellationToken: ct);

                return targetGroupsByIdResponse.Items
                    .Select(n =>
                        new Select2ResultItem(
                            n.TargetGroupId.ToString(),
                            n.TargetGroupName))
                    .ToArray();
            }

            int page;
            if (queryPage != null)
            {
                page = queryPage.Value;
            }
            else
            {
                page = 1;
            }

            ListTargetGroupsResponse profilesResponse =
                await adminClient.ListTargetGroupsAsync(
                    new ListTargetGroupsRequest
                    {
                        Term = term ?? string.Empty,
                        Offset = (page - 1) * PageSize,
                        // take 1 more so that we know there are more
                        Limit = PageSize + 1,
                    },
                    cancellationToken: ct);

            return new Select2Result(
                profilesResponse.Items
                    .Select(n =>
                        new Select2ResultItem(
                            n.TargetGroupId.ToString(),
                            n.TargetGroupName))
                    .ToArray(),
                new Select2ResultPagination(
                    profilesResponse.Items.Count > PageSize)
            );
        }

        public async Task<ActionResult<object>> RegisteredEntitiesAsync(
            [FromQuery(Name = "page")] int? queryPage,
            [FromServices] Admin.AdminClient adminClient,
            CancellationToken ct)
        {
            int page = queryPage ?? 1;

            GetRegisteredEntitiesResponse registeredEntitiesResponse =
                await adminClient.GetRegisteredEntitiesAsync(
                    new GetRegisteredEntitiesRequest()
                    {
                        Limit = PageSize + 1,
                        Offset = (page - 1) * PageSize
                    },
                    cancellationToken: ct);

            return new Select2Result(
                registeredEntitiesResponse.Result
                    .Select(n =>
                        new Select2ResultItem(
                            n.Identifier,
                            n.Name))
                    .ToArray(),
                new Select2ResultPagination(
                    registeredEntitiesResponse.Result.Count > PageSize)
            );
        }
    }
}
