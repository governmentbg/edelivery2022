using System;

namespace ED.EsbApi;

/// <summary>
///  Поле тип "select"
/// </summary>
/// <param name="Id">Идентификатор на поле</param>
/// <param name="Label">Наименование на поле</param>
/// <param name="IsEncrypted">Дали данните от полето се криптират</param>
/// <param name="IsRequired">Дали полето е задължително за попълване</param>
/// <param name="Value">Стойност на полето, използвайки стандарт ISO-8601</param>
/// <param name="Url">Url за зареждане на възможните стойности на полето</param>
/// <param name="Options">Списък с възможни стойности на полето</param>
public record SelectComponent(
    Guid Id,
    string Label,
    bool IsEncrypted,
    bool IsRequired,
    string? Value,
    string? Url,
    string[] Options)
    : ValueComponent(Id, Label, ComponentType.select, IsEncrypted, IsRequired, Value);
