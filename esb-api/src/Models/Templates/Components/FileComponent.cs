using System;

namespace ED.EsbApi;

/// <summary>
/// Поле тип "file"
/// </summary>
/// <param name="Id">Идентификатор на поле</param>
/// <param name="Label">Наименование на поле</param>
/// <param name="IsEncrypted">Дали данните от полето се криптират</param>
/// <param name="IsRequired">Дали полето е задължително за попълване</param>
/// <param name="MaxSize">Максимална големина на прикачения файл</param>
/// <param name="AllowedExtensions">Позволени вид файлове</param>
/// <param name="Instances">Максимален брой прикачени файлове</param>
public record FileComponent (
    Guid Id,
    string Label,
    bool IsEncrypted,
    bool IsRequired,
    int MaxSize, // TODO: missing ExpirationPeriod
    string? AllowedExtensions,
    int Instances)
    : BaseComponent(Id, Label, ComponentType.file, IsEncrypted, IsRequired);
