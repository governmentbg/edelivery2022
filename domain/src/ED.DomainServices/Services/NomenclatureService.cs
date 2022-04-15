using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ED.Domain;
using ED.DomainServices.Nomenclatures;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;

namespace ED.DomainServices
{
    public class NomenclatureService : Nomenclature.NomenclatureBase
    {
        private readonly IServiceProvider serviceProvider;
        public NomenclatureService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override async Task<GetEntityNomResponse> GetLoginSecurityLevel(
            GetNomRequest request,
            ServerCallContext context)
        {
            IEnumerable<EntityNomVO> loginSecurityLevels = await this.serviceProvider
                .GetRequiredService<ILoginSecurityLevelNomsRepository>()
                .GetNomsAsync(
                    request.Term,
                    request.Offset,
                    request.Limit,
                    context.CancellationToken
                );

            return new GetEntityNomResponse
            {
                Result =
                {
                    loginSecurityLevels.ProjectToType<EntityNom>()
                }
            };
        }

        public override async Task<GetEntityCodeNomResponse> GetCountries(
            GetNomRequest request,
            ServerCallContext context)
        {
            IEnumerable<EntityCodeNomVO> countries = await this.serviceProvider
               .GetRequiredService<ICountryNomsRepository>()
               .GetNomsAsync(
                   request.Term,
                   request.Offset,
                   request.Limit,
                   context.CancellationToken
               );

            return new GetEntityCodeNomResponse
            {
                Result =
                {
                    countries.ProjectToType<EntityCodeNom>()
                }
            };
        }

        public override async Task<GetActiveEntityNomResponse> GetTargetGroups(
            GetNomRequest request,
            ServerCallContext context)
        {
            IEnumerable<ActiveEntityNomVO> targetGroups = await this.serviceProvider
               .GetRequiredService<ITargetGroupNomsRepository>()
               .GetNomsAsync(
                   request.Term,
                   request.Offset,
                   request.Limit,
                   context.CancellationToken
               );

            return new GetActiveEntityNomResponse
            {
                Result =
                {
                    targetGroups.ProjectToType<ActiveEntityNom>()
                }
            };
        }
    }
}
