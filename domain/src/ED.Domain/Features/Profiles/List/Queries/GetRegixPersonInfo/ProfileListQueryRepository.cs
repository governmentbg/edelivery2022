using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IProfileListQueryRepository;
using static ED.Domain.RegixServiceClient;

namespace ED.Domain
{
    partial class ProfileListQueryRepository : IProfileListQueryRepository
    {
        public async Task<GetRegixPersonInfoVO?> GetRegixPersonInfoAsync(
            string identifier,
            CancellationToken ct)
        {
            RegixPersonInfoResponse? personInfo =
                await this.regixServiceClient.GetRegixPersonInfoAsync(
                    identifier,
                    ct);

            return personInfo != null
                ? new GetRegixPersonInfoVO(
                    personInfo.ErrorMessage,
                    personInfo.Success,
                    personInfo.FirstName!,
                    personInfo.SurName!,
                    personInfo.FamilyName!,
                    personInfo.BirthDate)
                : null;
        }
    }
}
