using System;

namespace ED.EsbApi;

/// <summary>
/// Модел за мета данни на съобщение
/// </summary>
/// <param name="DateCreated">Дата на вписване на мета данни</param>
/// <param name="Data">Списък мета данни</param>
public record MetaMessageDataDO(
    DateTime DateCreated,
    MetaMessageDataDOData[] Data);

/// <summary>
/// Модел за единица мета данни на съобщение
/// </summary>
/// <param name="Label">Етикет на мета данни</param>
/// <param name="Value">Стойност на мета данни</param>
public record MetaMessageDataDOData(
    string Label,
    string Value);

// TODO: add validator when functionality is implemented
