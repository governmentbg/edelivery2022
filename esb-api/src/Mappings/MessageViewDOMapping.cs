using System;
using System.Collections.Generic;
using Mapster;

namespace ED.EsbApi;

public class MessageViewDOMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DomainServices.Esb.ViewMessageResponse.Types.Message, MessageViewDO>()
            .Map(dest => dest.MessageId, src => src.MessageId)
            .Map(dest => dest.DateSent, src => src.DateSent.ToLocalDateTime())
            .Map(dest => dest.Recipients, src => src.Recipients)
            .Map(dest => dest.Subject, src => src.Subject)
            .Map(dest => dest.Rnu, src => src.Rnu)
            .Map(dest => dest.TemplateId, src => src.TemplateId)
            .Map(dest => dest.Fields, src => new Dictionary<Guid, object?>())
            .Map(dest => dest.ForwardedMessageId, src => src.ForwardedMessageId);

        config.NewConfig<DomainServices.Esb.ViewMessageResponse.Types.Blob, MessageViewDOBlob>()
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
