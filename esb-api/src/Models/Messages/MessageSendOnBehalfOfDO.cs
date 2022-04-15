using System;
using System.Collections.Generic;

namespace ED.EsbApi;

/// <summary>
/// Модел за изпращане на съобщение от името на чужд профил
/// </summary>
/// <param name="RecipientProfileIds">Списък идентификатори на получатели</param>
/// <param name="SenderProfileId">Идентификатор на изпращач</param>
/// <param name="Subject">Заглавие на съобщението</param>
/// <param name="ReferencedOrn">Към ORN</param>
/// <param name="AdditionalIdentifier">Допълнителен идентификатор на съобщение</param>
/// <param name="TemplateId">Шаблон на съобщението</param>
/// <param name="Fields">Списък с полетата и техните стойности в шаблона на съобщението, изключвайки прикачените документи, във формат (Идентификатор на поле, Стойност)</param>
/// <param name="Blobs">Списък с файловите полета в шаблона на съобщението във формат (Идентификатор на файлово поле, Стойност от хранилището)</param>
public record MessageSendOnBehalfOfDO(
    int[] RecipientProfileIds,
    int SenderProfileId,
    string Subject,
    string? ReferencedOrn,
    string? AdditionalIdentifier,
    int TemplateId,
    Dictionary<Guid, string?> Fields,
    Dictionary<Guid, int[]> Blobs);
