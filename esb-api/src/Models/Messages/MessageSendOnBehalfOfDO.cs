using System;
using System.Collections.Generic;

namespace ED.EsbApi;

/// <summary>
/// Модел за изпращане на съобщение от името на чужд профил
/// </summary>
/// <param name="RecipientProfileIds">Списък идентификатори на получатели</param>
/// <param name="SenderProfileId">Идентификатор на изпращач</param>
/// <param name="Subject">Заглавие на съобщението</param>
/// <param name="Rnu">Референтен номер на услуга (РНУ)</param>
/// <param name="TemplateId">Шаблон на съобщението</param>
/// <param name="Fields">Списък с полетата и техните стойности в шаблона на съобщението във формат (Идентификатор на поле, Стойност)</param>
public record MessageSendOnBehalfOfDO(
    int[] RecipientProfileIds,
    int SenderProfileId,
    string Subject,
    string? Rnu,
    int TemplateId,
    Dictionary<Guid, object?> Fields);

// TODO: add validator
