GO
PRINT 'MessageTranslationRequests'
GO

CREATE TABLE [dbo].[MessageTranslationRequests](
  [MessageTranslationId] [int] NOT NULL,
  [MessageTranslationRequestId] [int] IDENTITY(1,1) NOT NULL,
  [RequestId] [bigint] NULL,
  [SourceBlobId] [int] NULL,
  [TargetBlobId] [int] NULL,
  [Status] [int] NOT NULL,
  [ErrorMessage] [nvarchar](MAX) NULL,
  CONSTRAINT [PK_MessageTranslationRequests] PRIMARY KEY ([MessageTranslationId], [MessageTranslationRequestId]),
  CONSTRAINT [FK_MessageTranslationRequests_MessageTranslations] FOREIGN KEY([MessageTranslationId]) REFERENCES [dbo].[MessageTranslations] ([MessageTranslationId]),
  CONSTRAINT [FK_MessageTranslationRequests_Blobs_SourceBlobId] FOREIGN KEY([SourceBlobId]) REFERENCES [dbo].[Blobs] ([BlobId]),
  CONSTRAINT [FK_MessageTranslationRequests_Blobs_TargetBlobId] FOREIGN KEY([TargetBlobId]) REFERENCES [dbo].[Blobs] ([BlobId])
);
GO
