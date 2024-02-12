using System;
using System.Collections.Generic;

namespace ED.EsbApi;

/// <summary>
/// Модел за изпращане на съобщение с код
/// </summary>
/// <param name="FirstName">Собствено име на получател</param>
/// <param name="MiddleName">Бащино име на получател</param>
/// <param name="LastName">Фамилия на получател</param>
/// <param name="Identifier">Идентификатор (ЕГН/ЛНЧ) на получател</param>
/// <param name="Email">Имейл на получател</param>
/// <param name="Phone">Телефона на получател</param>
/// <param name="Subject">Заглавие на съобщение</param>
/// <param name="Body">Съдържание на съобщение</param>
/// <param name="Blobs">Списък с файловите полета в шаблона на съобщението във формат (Идентификатор на файлово поле, Стойност от хранилището)</param>
public record CodeMessageSendDO(
    string FirstName,
    string? MiddleName,
    string LastName,
    string Identifier,
    string Email,
    string Phone,
    string Subject,
    string Body,
    Dictionary<Guid, int[]> Blobs);

// TODO: add validator when functionality is implemented
