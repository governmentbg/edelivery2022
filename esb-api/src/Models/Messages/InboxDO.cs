using System;

namespace ED.EsbApi;

/// <summary>
/// Получено съобщение
/// </summary>
/// <param name="MessageId">Идентификатор на съобщение</param>
/// <param name="DateSent">Дата на изпращане</param>
/// <param name="DateReceived">Дата получаване/отваряне</param>
/// <param name="Subject">Заглавие на съобщение</param>
/// <param name="SenderProfileName">Наименование на профила изпращач</param>
/// <param name="SenderLoginName">Наименование на потребител, изпратил съобщението</param>
/// <param name="RecipientProfileName">Наименование на профила получател</param>
/// <param name="RecipientLoginName">Наименование на потребител, получил съобщението</param>
/// <param name="Url">Url за преглед на съобщението в ССЕВ</param>
/// <param name="Rnu">Референтен номер на услуга (РНУ)</param>
public record InboxDO(
    int MessageId,
    DateTime DateSent,
    DateTime? DateReceived,
    string Subject,
    string SenderProfileName,
    string SenderLoginName,
    string RecipientProfileName,
    string RecipientLoginName,
    string Url,
    string? Rnu);
