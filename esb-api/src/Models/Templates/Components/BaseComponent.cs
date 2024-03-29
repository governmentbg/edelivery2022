﻿using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NJsonSchema.Converters;

namespace ED.EsbApi;

/// <summary>
/// Базов клас за поле в шаблон. 
/// Спрямо полето "discriminator" се сериализира до следните класове: CheckboxComponent, FileComponent, DateTimeComponent, HiddenComponent, SelectComponent, TextAreaComponent, TextFieldComponent и MarkdownComponent.
/// </summary>
/// <param name="Id">Идентификатор на поле</param>
/// <param name="Label">Наименование на поле</param>
/// <param name="Type">Тип на поле</param>
/// <param name="IsEncrypted">Дали данните от полето се криптират</param>
/// <param name="IsRequired">Дали полето е задължително за попълване</param>
[JsonConverter(typeof(JsonInheritanceConverter), "discriminator")]
[KnownType(typeof(CheckboxComponent))]
[KnownType(typeof(FileComponent))]
[KnownType(typeof(DateTimeComponent))]
[KnownType(typeof(HiddenComponent))]
[KnownType(typeof(SelectComponent))]
[KnownType(typeof(TextAreaComponent))]
[KnownType(typeof(TextFieldComponent))]
[KnownType(typeof(MarkdownComponent))]
public abstract record BaseComponent(
    Guid Id,
    string Label,
    [property: JsonConverter(typeof(StringEnumConverter))] ComponentType Type,
    bool IsEncrypted,
    bool IsRequired);
