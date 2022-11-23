using System;

namespace ED.EsbApi;

public abstract record ValueComponent(
    Guid Id,
    string Label,
    ComponentType Type,
    bool IsEncrypted,
    bool IsRequired,
    string? Value)
    : BaseComponent(Id, Label, Type, IsEncrypted, IsRequired);
