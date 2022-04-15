using System;

namespace ED.EsbApi;

/// <summary>
/// Изпратено съобщение
/// </summary>
/// <param name="MessageId">Идентификатор на съобщение</param>
/// <param name="DateSent">Дата на изпращане</param>
/// <param name="Subject">Заглавие на съобщение</param>
/// <param name="SenderProfileName">Наименование на профила изпращач</param>
/// <param name="SenderLoginName">Наименование на потребител, изпратил съобщението</param>
/// <param name="Recipients">Получатели като текст</param>
/// <param name="Url">Url за преглед на съобщението в ССЕВ</param>
/// <param name="Orn">Референтен номер на операция</param>
/// <param name="ReferencedOrn">Към ORN</param>
/// <param name="AdditionalIdentifier">Допълнителен идентификатор на съобщение</param>
public record OutboxDO(
    int MessageId,
    DateTime DateSent,
    string Subject,
    string SenderProfileName,
    string SenderLoginName,
    string Recipients,
    string Url,
    string? Orn,
    string? ReferencedOrn,
    string? AdditionalIdentifier);
