using System;

namespace ED.EsbApi;

/// <summary>
/// Файл/документ
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
public record BlobDO(
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
public record BlobDOSignature(
    bool CoversDocument,
    DateTime SignDate,
    bool IsTimestamp,
    bool ValidAtTimeOfSigning,
    string Issuer,
    string Subject,
    DateTime ValidFrom,
    DateTime ValidTo);
