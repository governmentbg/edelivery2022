using System;
using System.Collections.Generic;

namespace ED.EsbApi;

/// <summary>
/// Данни за получено съобщение
/// </summary>
/// <param name="MessageId">Идентификатор на съобщение</param>
/// <param name="DateSent">Дата на изпращане</param>
/// <param name="Sender">Профил изпращач</param>
/// <param name="DateReceived">Дата на отваряне на съобщението</param>
/// <param name="RecipientLogin">Потребител отворил съобщението</param>
/// <param name="Subject">Заглавие на съобщение</param>
/// <param name="Orn">Референтен номер на операция</param>
/// <param name="ReferencedOrn">Към ORN</param>
/// <param name="AdditionalIdentifier">Допълнителен идентификатор на съобщение</param>
/// <param name="TemplateId">Идентификатор на шаблон на съобщение</param>
/// <param name="Fields">Списък с полета и техните стойности в шаблона на съобщението, изключвайки прикачените документи, във формат (Идентификатор на поле, Стойност)</param>
/// <param name="Blobs">Списък с <see cref="MessageOpenDOBlob">файлови полета</see> и техните стойности в шаблона на съобщението</param>
/// <param name="ForwardedMessageId">Идентификатор на препратено съобщение</param>
public record MessageOpenDO(
    int MessageId,
    DateTime DateSent,
    MessageOpenDOSenderProfile Sender,
    DateTime? DateReceived,
    MessageOpenDORecipientLogin? RecipientLogin,
    string Subject,
    string? Orn,
    string? ReferencedOrn,
    string? AdditionalIdentifier,
    int TemplateId,
    Dictionary<Guid, string?> Fields,
    MessageOpenDOBlob[] Blobs,
    int? ForwardedMessageId);

/// <summary>
/// Профил, изпращач на съобщение
/// </summary>
/// <param name="ProfileId">Идентификатор на профил</param>
/// <param name="Name">Наименование на профил</param>
public record MessageOpenDOSenderProfile(
    int ProfileId,
    string Name);

/// <summary>
/// Потребител, отворил съобщението
/// </summary>
/// <param name="LoginId">Идентификатор на потребител</param>
/// <param name="Name">Наименование на потребител</param>
public record MessageOpenDORecipientLogin(
    int LoginId,
    string Name);

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
public record MessageOpenDOBlob(
    int BlobId,
    string FileName,
    long? Size,
    string? DocumentRegistrationNumber,
    bool? IsMalicious,
    string? Hash,
    string? HashAlgorithm,
    string DownloadLink,
    DateTime DownloadLinkExpirationDate,
    MessageOpenDOBlobSignature[] Signatures);

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
public record MessageOpenDOBlobSignature(
    bool CoversDocument,
    DateTime SignDate,
    bool IsTimestamp,
    bool ValidAtTimeOfSigning,
    string Issuer,
    string Subject,
    DateTime ValidFrom,
    DateTime ValidTo);
