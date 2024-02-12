using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
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

        this.RuleFor(x => x.RecipientProfileIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
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
                IList<BaseComponent> components =
                    await templateService.GetTemplateComponentsAsync(
                        compose.TemplateId,
                        ct);

                foreach (var field in compose.Fields)
                {
                    var match = components.FirstOrDefault(e => e.Id == field.Key);

                    // check require rule
                    if (match == null
                        || (match.IsRequired && field.Value == null))
                    {
                        return false;
                    }

                    // check value type
                    switch (match.Type)
                    {
                        case ComponentType.textfield:
                        case ComponentType.textarea:
                        case ComponentType.select:
                        case ComponentType.hidden:
                        case ComponentType.datetime:
                            if (field.Value != null && field.Value is not string)
                            {
                                return false;
                            }

                            break;
                        case ComponentType.checkbox:
                            if (field.Value != null && field.Value is not bool)
                            {
                                return false;
                            }

                            break;
                        case ComponentType.file:
                            if (field.Value != null)
                            {
                                try
                                {
                                    int[] deserializedValues = ((JArray)field.Value).ToObject<int[]>()!;
                                }
#pragma warning disable CA1031 // Do not catch general exception types
                                catch
#pragma warning restore CA1031 // Do not catch general exception types
                                {
                                    return false;
                                }
                            }

                            break;
                        case ComponentType.markdown:
                            break;
                        default:
                            return false;
                    }

                }

                return true;
            })
            .WithMessage("Invalid fields according to template");
    }
}
