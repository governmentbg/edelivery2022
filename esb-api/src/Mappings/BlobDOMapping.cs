using System;
using Mapster;

namespace ED.EsbApi;

public class BlobDOMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DomainServices.Esb.GetStorageBlobsResponse.Types.Blob, BlobDO>()
            .Map(dest => dest.BlobId, src => src.BlobId)
            .Map(dest => dest.FileName, src => src.FileName)
            .Map(dest => dest.Size, src => src.Size)
            .Map(dest => dest.DocumentRegistrationNumber, src => src.DocumentRegistrationNumber)
            .Map(dest => dest.IsMalicious, src => src.IsMalicious)
            .Map(dest => dest.Hash, src => src.Hash)
            .Map(dest => dest.HashAlgorithm, src => src.HashAlgorithm)
            .Map(dest => dest.DownloadLink, src => string.Empty)
            .Map(dest => dest.DownloadLinkExpirationDate, src => DateTime.Now)
            .Map(dest => dest.Signatures, src => src.Signatures);

        config.NewConfig<DomainServices.Esb.GetStorageBlobInfoResponse.Types.Blob, BlobDO>()
            .Map(dest => dest.BlobId, src => src.BlobId)
            .Map(dest => dest.FileName, src => src.FileName)
            .Map(dest => dest.Size, src => src.Size)
            .Map(dest => dest.DocumentRegistrationNumber, src => src.DocumentRegistrationNumber)
            .Map(dest => dest.IsMalicious, src => src.IsMalicious)
            .Map(dest => dest.Hash, src => src.Hash)
            .Map(dest => dest.HashAlgorithm, src => src.HashAlgorithm)
            .Map(dest => dest.DownloadLink, src => string.Empty)
            .Map(dest => dest.DownloadLinkExpirationDate, src => DateTime.Now)
            .Map(dest => dest.Signatures, src => src.Signatures);
    }
}
