using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ED.Domain
{
    public abstract class EDModule : Autofac.Module
    {
        private IConfiguration configuration;
        private IWebHostEnvironment environment;

        protected EDModule(
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            this.configuration = configuration;
            this.environment = environment;
        }

        protected virtual void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
        }

        protected virtual void ConfigureAutofacServices(
            ContainerBuilder builder,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            this.ConfigureServices(services, this.configuration, this.environment);
        }

        protected override void Load(ContainerBuilder builder)
        {
            this.ConfigureAutofacServices(builder, this.configuration, this.environment);
        }
    }
}
