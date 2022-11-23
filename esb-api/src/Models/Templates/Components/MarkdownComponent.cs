using System;

namespace ED.EsbApi;

/// <summary>
///  Поле тип "markdown"
/// </summary>
/// <param name="Id">Идентификатор на поле</param>
/// <param name="Label">Наименование на поле</param>
/// <param name="IsEncrypted">Дали данните от полето се криптират</param>
/// <param name="IsRequired">Дали полето е задължително за попълване</param>
/// <param name="Value">Стойност на полето</param>
/// <param name="PdfValue">Стойност на полето в PDF експорт</param>
public record MarkdownComponent(
    Guid Id,
    string Label,
    bool IsEncrypted,
    bool IsRequired,
    string? Value,
    string? PdfValue)
    : ValueComponent(Id, Label, ComponentType.textfield, IsEncrypted, IsRequired, Value);
