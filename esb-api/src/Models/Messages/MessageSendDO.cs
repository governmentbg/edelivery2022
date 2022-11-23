using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

/// <summary>
/// Модел за изпращане на съобщение
/// </summary>
/// <param name="RecipientProfileIds">Списък идентификатори на получатели</param>
/// <param name="Subject">Заглавие на съобщението</param>
/// <param name="Rnu">Референтен номер на услуга (РНУ)</param>
/// <param name="TemplateId">Шаблон на съобщението</param>
/// <param name="Fields">Списък с полетата и техните стойности в шаблона на съобщението във формат (Идентификатор на поле, Стойност)</param>
public record MessageSendDO(
    int[] RecipientProfileIds,
    string Subject,
    string? Rnu,
    int TemplateId,
    Dictionary<Guid, object?> Fields);

public class MessageSendDOValidator : AbstractValidator<MessageSendDO>
{
    private readonly HttpContext httpContext;
    private readonly EsbClient esbClient;
    private readonly ITemplateService templateService;

    public MessageSendDOValidator(
        IHttpContextAccessor httpContextAccessor,
        EsbClient esbClient,
        ITemplateService templateService)
    {
        if (httpContextAccessor.HttpContext == null)
        {
            throw new Exception($"'{nameof(MessageSendDOValidator)}' called outside of a request. IHttpContextAccessor.HttpContext is null!");
        }

        this.httpContext = httpContextAccessor.HttpContext;
        this.esbClient = esbClient;
        this.templateService = templateService;

        this.RuleFor(x => x.RecipientProfileIds).NotEmpty();
        this.RuleFor(x => x.RecipientProfileIds)
            .MustAsync(async (it, profileIds, context, ct) =>
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
        this.RuleFor(x => new { x.TemplateId, x.Fields })
            .MustAsync(async (compose, ct) =>
            {
                // TODO: unfinished validation
                IList<BaseComponent> components =
                    await templateService.GetTemplateComponentsAsync(
                        compose.TemplateId,
                        ct);

                foreach (var field in compose.Fields)
                {
                    // TODO: switch for each component type
                    var match = components.FirstOrDefault(e => e.Id == field.Key);

                    if (match == null)
                    {
                        return false;
                    }

                    if (match.IsRequired && field.Value == null)
                    {
                        return false;
                    }
                }

                return true;
            })
            .WithMessage("Invalid fields or blobs according to template");
    }
}
