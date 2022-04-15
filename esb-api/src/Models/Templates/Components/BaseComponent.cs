using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NJsonSchema.Converters;

namespace ED.EsbApi;

/// <summary>
/// Базов клас за поле в шаблон
/// </summary>
/// <param name="Id">Идентификатор на поле</param>
/// <param name="Label">Наименование на поле</param>
/// <param name="Type">Тип на поле</param>
/// <param name="IsEncrypted">Дали данните от полето се криптират</param>
/// <param name="IsRequired">Дали полето е задължително за попълване</param>
[JsonConverter(typeof(JsonInheritanceConverter), "discriminator")]
[KnownType(typeof(CheckboxComponent))]
[KnownType(typeof(FileComponent))]
[KnownType(typeof(ValueComponent))]
public abstract record BaseComponent(
    Guid Id,
    string Label,
    [property: JsonConverter(typeof(StringEnumConverter))] ComponentType Type,
    bool IsEncrypted,
    bool IsRequired);
