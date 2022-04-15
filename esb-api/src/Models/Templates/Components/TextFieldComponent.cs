using System;

namespace ED.EsbApi;

/// <summary>
///  Поле тип "textfield"
/// </summary>
/// <param name="Id">Идентификатор на поле</param>
/// <param name="Label">Наименование на поле</param>
/// <param name="IsEncrypted">Дали данните от полето се криптират</param>
/// <param name="IsRequired">Дали полето е задължително за попълване</param>
/// <param name="Value">Стойност на полето</param>
public record TextFieldComponent(
    Guid Id,
    string Label,
    bool IsEncrypted,
    bool IsRequired,
    string? Value)
    : ValueComponent(Id, Label, ComponentType.textfield, IsEncrypted, IsRequired, Value);
