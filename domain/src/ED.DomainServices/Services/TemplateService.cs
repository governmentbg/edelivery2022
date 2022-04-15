using System;
using System.Threading.Tasks;
using ED.Domain;
using ED.DomainServices.Templates;
using Grpc.Core;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace ED.DomainServices
{
    public class TemplateService : Templates.Template.TemplateBase
    {
        private readonly IServiceProvider serviceProvider;
        public TemplateService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override async Task<GetContentResponse> GetContent(
            GetContentRequest request,
            ServerCallContext context)
        {
            ITemplateListQueryRepository.GetContentVO template =
                await this.serviceProvider
                    .GetRequiredService<ITemplateListQueryRepository>()
                    .GetContentAsync(
                        request.TemplateId,
                        context.CancellationToken);

            return template.Adapt<GetContentResponse>();
        }
    }
}
