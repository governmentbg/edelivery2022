using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ED.EsbApi;

public interface ITemplateService
{
    Task<IList<BaseComponent>> GetTemplateComponentsAsync(int templateId, CancellationToken ct);
}
