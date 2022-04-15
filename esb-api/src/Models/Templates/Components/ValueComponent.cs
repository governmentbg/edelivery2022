using System;
using System.Runtime.Serialization;

namespace ED.EsbApi;

[KnownType(typeof(DateTimeComponent))]
[KnownType(typeof(HiddenComponent))]
[KnownType(typeof(SelectComponent))]
[KnownType(typeof(TextAreaComponent))]
[KnownType(typeof(TextFieldComponent))]
public abstract record ValueComponent(
    Guid Id,
    string Label,
    ComponentType Type,
    bool IsEncrypted,
    bool IsRequired,
    string? Value)
    : BaseComponent(Id, Label, Type, IsEncrypted, IsRequired);
