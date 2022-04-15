using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;

namespace ED.PdfServices
{
    public class WebApiExceptionLogger : IExceptionLogger
    {
        private readonly ILogger<WebApiExceptionLogger> logger;
        public WebApiExceptionLogger(ILogger<WebApiExceptionLogger> logger)
        {
            this.logger = logger;
        }

        public virtual Task LogAsync(
            ExceptionLoggerContext context,
            CancellationToken cancellationToken)
        {
            var req = context.Request;
            logger.LogError(context.Exception, $"{req.Method} {req.RequestUri} failed.");
            return Task.CompletedTask;
        }
    }
}
