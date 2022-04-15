using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using ED.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ED.DomainJobs
{
    public class EmailJob : QueueJob<EmailQueueMessage, SmtpClient>
    {
        private string mailServer;
        private string mailSender;
        private string? mailServerUsername;
        private string? mailServerPassword;
        private string? mailServerDomain;

        public EmailJob(
            IServiceScopeFactory scopeFactory,
            ILogger<EmailJob> logger,
            IOptions<DomainJobsOptions> optionsAccessor)
            : base(QueueMessageType.Email, scopeFactory, logger, optionsAccessor.Value.EmailJob)
        {
            var options = optionsAccessor.Value.EmailJob;
            this.mailServer = options.MailServer;
            this.mailSender = options.MailSender;
            this.mailServerUsername = options.MailServerUsername;
            this.mailServerPassword = options.MailServerPassword;
            this.mailServerDomain = options.MailServerDomain;
        }

        protected override Task<SmtpClient> CreateThreadContextAsync(CancellationToken ct)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            // the job will dispose of the ThreadContext once its finished with
            // all the items assigned for this thread
            var smtpClient = new SmtpClient(this.mailServer);
#pragma warning restore CA2000 // Dispose objects before losing scope

            if (!string.IsNullOrEmpty(this.mailServerUsername))
            {
                var credentials = new NetworkCredential(this.mailServerUsername, this.mailServerPassword);

                if (!string.IsNullOrEmpty(this.mailServerDomain))
                {
                    credentials.Domain = this.mailServerDomain;
                }

                smtpClient.Credentials = credentials;
            }
            
            return Task.FromResult(smtpClient);
        }

        protected override async Task<(QueueJobProcessingResult result, string? error)> HandleMessageAsync(
            SmtpClient context,
            EmailQueueMessage payload,
            CancellationToken ct)
        {
            var smtpClient = context;

            try
            {
                MailAddress from = new(this.mailSender);
                MailAddress to = new(payload.Recipient);

                using MailMessage mailMessage = new(from, to)
                {
                    Subject = payload.Subject,
                    Body = payload.Body,
                    SubjectEncoding = System.Text.Encoding.UTF8,
                    BodyEncoding = System.Text.Encoding.UTF8,
                    IsBodyHtml = payload.IsBodyHtml,
                    DeliveryNotificationOptions = DeliveryNotificationOptions.None,
                };

                await smtpClient.SendMailAsync(mailMessage, ct);

                return (QueueJobProcessingResult.Success, null);
            }
            catch (SmtpException smtpEx)
            {
                var error = string.Format(
                    "SmtpException: code->{0}, message->{1}, source->{2}, stacktrace->{3}",
                    Enum.GetName(typeof(SmtpStatusCode), smtpEx.StatusCode),
                    smtpEx.Message,
                    smtpEx.Source,
                    smtpEx.StackTrace);

                this.Logger.Log(LogLevel.Warning, error);

                return (QueueJobProcessingResult.RetryError, error);
            }
        }
    }
}
