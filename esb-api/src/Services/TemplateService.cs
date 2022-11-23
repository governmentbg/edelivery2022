using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

public class TemplateService : ITemplateService
{
    private readonly EsbClient esbClient;

    public TemplateService(EsbClient esbClient)
    {
        this.esbClient = esbClient;
    }

    public async Task<IList<BaseComponent>> GetTemplateComponentsAsync(
        int templateId,
        CancellationToken ct)
    {
        DomainServices.Esb.GetTemplateResponse resp =
           await this.esbClient.GetTemplateAsync(
               new DomainServices.Esb.GetTemplateRequest
               {
                   TemplateId = templateId,
               },
               cancellationToken: ct);

        List<BaseComponent> components =
            JsonConvert.DeserializeObject<List<BaseComponent>>(
                resp.Result.Content,
                new TemplateComponentConverter())
                    ?? new List<BaseComponent>();

        return components;
    }
}
