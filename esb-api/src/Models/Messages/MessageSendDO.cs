using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

/// <summary>
/// Модел за изпращане на съобщение
/// </summary>
/// <param name="RecipientProfileIds">Списък идентификатори на получатели</param>
/// <param name="Subject">Заглавие на съобщението</param>
/// <param name="ReferencedOrn">Към ORN</param>
/// <param name="AdditionalIdentifier">Допълнителен идентификатор на съобщение</param>
/// <param name="TemplateId">Шаблон на съобщението</param>
/// <param name="Fields">Списък с полетата и техните стойности в шаблона на съобщението, изключвайки прикачените документи, във формат (Идентификатор на поле, Стойност)</param>
/// <param name="Blobs">Списък с файловите полета в шаблона на съобщението във формат (Идентификатор на файлово поле, Стойност от хранилището)</param>
/// <param name="ForwardedMessageId">Идентификатор на препратеното съобщение</param>
public record MessageSendDO(
    int[] RecipientProfileIds,
    string Subject,
    string? ReferencedOrn,
    string? AdditionalIdentifier,
    int TemplateId,
    Dictionary<Guid, string?> Fields,
    Dictionary<Guid, int[]> Blobs,
    int? ForwardedMessageId);

public class MessageSendDOValidator : AbstractValidator<MessageSendDO>
{
    private readonly HttpContext httpContext;
    private readonly EsbClient esbClient;

    public MessageSendDOValidator(
        IHttpContextAccessor httpContextAccessor,
        EsbClient esbClient)
    {
        if (httpContextAccessor.HttpContext == null)
        {
            throw new Exception($"'{nameof(MessageSendDOValidator)}' called outside of a request. IHttpContextAccessor.HttpContext is null!");
        }

        this.httpContext = httpContextAccessor.HttpContext;
        this.esbClient = esbClient;

        this.RuleFor(x => x.RecipientProfileIds).NotEmpty();
        this.RuleFor(x => x.RecipientProfileIds)
            .MustAsync(async (profileIds, ct) =>
            {
                int profileId = this.httpContext.User.GetAuthenticatedUserProfileId();

                DomainServices.Esb.CheckMessageRecipientsResponse resp =
                   await this.esbClient.CheckMessageRecipientsAsync(
                       new DomainServices.Esb.CheckMessageRecipientsRequest
                       {
                           RecipientProfileIds =
                           {
                               profileIds
                           },
                           ProfileId = profileId,
                       },
                       cancellationToken: ct);

                return resp.IsOk;
            })
            .WithMessage("Can not send to recipients");
        this.RuleFor(x => x.Subject).NotEmpty();
        this.RuleFor(x => new { x.TemplateId, x.Fields, x.Blobs })
            .MustAsync(async (compose, ct) =>
            {
                DomainServices.Esb.GetTemplateResponse resp =
                   await this.esbClient.GetTemplateAsync(
                       new DomainServices.Esb.GetTemplateRequest
                       {
                           TemplateId = compose.TemplateId,
                       },
                       cancellationToken: ct);

                List<BaseComponent> components =
                    JsonConvert.DeserializeObject<List<BaseComponent>>(
                        resp.Result.Content,
                        new TemplateComponentConverter())
                        ?? new List<BaseComponent>();

                foreach (var field in compose.Fields)
                {
                    var match = components.FirstOrDefault(e => e.Id == field.Key);

                    if (match == null)
                    {
                        return false;
                    }

                    if (match.IsRequired && string.IsNullOrEmpty(field.Value))
                    {
                        return false;
                    }
                }

                // TODO: files and their owner?

                return true;
            })
            .WithMessage("Invalid fields or blobs according to template");
    }
}
