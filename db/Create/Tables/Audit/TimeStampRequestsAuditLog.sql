GO
PRINT 'TimeStampRequestsAuditLog'
GO

CREATE TABLE [dbo].[TimeStampRequestsAuditLog](
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [MessageId] [int] NOT NULL,
  [BlobId] [int] NULL,
  [DateSent] [datetime] NOT NULL,
  [Status] [int] NOT NULL,
  [Description] [nvarchar](MAX) NULL,

  CONSTRAINT [PK_TimeStampRequestsAuditLog] PRIMARY KEY ([Id]),
  CONSTRAINT [CHK_TimeStampRequestsAuditLog_Status] CHECK ([Status] IN (1, 2, 3)),
)
GO
