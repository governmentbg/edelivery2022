using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace ED.Domain
{
    public static class SBModuleExtensions
    {
        public static void RegisterModules(
            this IServiceCollection services,
            IEnumerable<EDModule> modules)
        {
            foreach (var module in modules)
            {
                module.ConfigureServices(services);
            }
        }
    }
}
