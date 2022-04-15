﻿namespace ED.EsbApi;

/// <summary>
/// Шаблон на съобщение
/// </summary>
/// <param name="TemplateId">Идентификатор на шаблон</param>
/// <param name="Name">Наименование на шаблон</param>
/// <param name="IdentityNumber">Номер на шаблон</param>
/// <param name="Read">Мин. ниво на осигуреност нужно за получаване на съобщение със съответния шаблон</param>
/// <param name="Write">Мин. ниво на осигуреност нужно за изпращане на съобщение със съответния шаблон</param>
/// <param name="ResponseTemplateId">Шаблон на съобщение при отговор</param>
public record TemplateDO(
    int TemplateId,
    string Name,
    string IdentityNumber,
    TemplateSecurityLevel Read,
    TemplateSecurityLevel Write,
    int? ResponseTemplateId);
