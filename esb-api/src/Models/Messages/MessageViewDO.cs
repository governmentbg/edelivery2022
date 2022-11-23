using System;
using System.Collections.Generic;

namespace ED.EsbApi;

/// <summary>
/// Данни за изпратено съобщение
/// </summary>
/// <param name="MessageId">Идентификатор на съобщение</param>
/// <param name="DateSent">Дата на изпращане</param>
/// <param name="Recipients">Списък с <see cref="MessageViewDORecipient">получатели</see> на съобщението</param>
/// <param name="Subject">Заглавие на съобщение</param>
/// <param name="Rnu">Референтен номер на услуга (РНУ)</param>
/// <param name="TemplateId">Идентификатор на шаблон на съобщение</param>
/// <param name="Fields">Списък с полета и техните стойности в шаблона на съобщението във формат (Идентификатор на поле, Стойност)</param>
/// <param name="ForwardedMessageId">Идентификатор на препратено съобщение</param>
public record MessageViewDO(
    int MessageId,
    DateTime DateSent,
    MessageViewDORecipient[] Recipients,
    string Subject,
    string? Rnu,
    int TemplateId,
    Dictionary<Guid, object?> Fields,
    int? ForwardedMessageId);

/// <summary>
/// Получател на съобщение
/// </summary>
/// <param name="ProfileId">Идентификатор на профил получател</param>
/// <param name="Name">Наименование на профил получател</param>
/// <param name="DateReceived">Дата на получаване на съобщение</param>
public record MessageViewDORecipient(
    int ProfileId,
    string Name,
    DateTime? DateReceived);

/// <summary>
/// Прикачен файл към съобщение
/// </summary>
/// <param name="BlobId">Идентификатор на файл</param>
/// <param name="FileName">Наименование на файл</param>
/// <param name="Size">Големина на файл (bytes)</param>
/// <param name="DocumentRegistrationNumber">Регистрационен номер на документ</param>
/// <param name="IsMalicious">Флаг показващ дали файла съдържа зловреден код</param>
/// <param name="Hash">Хаш на файл</param>
/// <param name="HashAlgorithm">Алгоритъм за пресмятане на хаш на файл</param>
/// <param name="DownloadLink">Линк за сваляне на файл</param>
/// <param name="DownloadLinkExpirationDate">Дата и час за валидност на линка за сваляне на файл</param>
/// <param name="Signatures">Списък ел. подписи, извлечени от файла</param>
public record MessageViewDOBlob(
    int BlobId,
    string FileName,
    long? Size,
    string? DocumentRegistrationNumber,
    bool? IsMalicious,
    string? Hash,
    string? HashAlgorithm,
    string DownloadLink,
    DateTime DownloadLinkExpirationDate,
    MessageViewDOBlobSignature[] Signatures);

/// <summary>
/// Електронен подпис към файл
/// </summary>
/// <param name="CoversDocument"></param>
/// <param name="SignDate"></param>
/// <param name="IsTimestamp"></param>
/// <param name="ValidAtTimeOfSigning"></param>
/// <param name="Issuer"></param>
/// <param name="Subject"></param>
/// <param name="ValidFrom"></param>
/// <param name="ValidTo"></param>
public record MessageViewDOBlobSignature(
    bool CoversDocument,
    DateTime SignDate,
    bool IsTimestamp,
    bool ValidAtTimeOfSigning,
    string Issuer,
    string Subject,
    DateTime ValidFrom,
    DateTime ValidTo);
