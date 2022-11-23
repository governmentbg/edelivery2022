GO
PRINT 'Messages'
GO

CREATE TABLE [dbo].[Messages](
  [MessageId] [int] IDENTITY(1,1) NOT NULL,
  [SenderProfileId] [int] NOT NULL,
  [SenderLoginId] [int] NULL,
  [Subject] [nvarchar](255) NOT NULL,
  [Body] [varbinary](max) NULL,
  [MessageSummary] [varbinary](MAX) NULL,
  [MessageSummaryVersion] [int] NULL,
  [DateSent] [datetime] NULL,
  [TimeStampNRO] [varbinary](8000) NULL,
  [CreatedBy] [int] NOT NULL,
  [DateCreated] [datetime] NOT NULL,
  [SentViaLoginId] [int] NULL,
  [SubjectExtended] [nvarchar](1024) NULL,
  [ForwardStatusId] [tinyint] NOT NULL DEFAULT (0),
  [IV] [binary](16) NULL,
  [MessageSummaryXml] [xml] NULL,
  [MetaFields] [nvarchar](max) NULL,
  [TemplateId] [int] NULL,
  [RecipientsAsText] [nvarchar](max) NOT NULL,
  [MessagePdfBlobId] [int] NULL,
  [Rnu] [NVARCHAR](64) NULL,

  CONSTRAINT [PK_Messages] PRIMARY KEY ([MessageId]),
  CONSTRAINT [FK_Messages_Blobs_MessagePdfBlobId]
    FOREIGN KEY([MessagePdfBlobId])
    REFERENCES [dbo].[Blobs] ([BlobId]),
  CONSTRAINT [FK_Messages_MessageCreatedByLogin]
    FOREIGN KEY([CreatedBy])
    REFERENCES [dbo].[Logins] ([Id]),
  CONSTRAINT [FK_Messages_SenderLogin]
    FOREIGN KEY([SenderLoginId])
    REFERENCES [dbo].[Logins] ([Id]),
  CONSTRAINT [FK_Messages_SenderProfile]
    FOREIGN KEY([SenderProfileId])
    REFERENCES [dbo].[Profiles] ([Id]),
  CONSTRAINT [FK_Messages_Templates]
    FOREIGN KEY([TemplateId])
    REFERENCES [dbo].[Templates] ([TemplateId]),
  CONSTRAINT [CHK_Messages_MessageSummaryVersion]
    CHECK (
      [MessageSummaryVersion] IS NULL
      OR [MessageSummaryVersion] IN (1, 2)),
)
GO

CREATE NONCLUSTERED INDEX [IX_Messages_SenderProfileId]
ON [dbo].[Messages] ([SenderProfileId])
GO

CREATE NONCLUSTERED INDEX [IX_Messages_MessagePdfBlobId]
ON [dbo].[Messages] ([MessagePdfBlobId])
WHERE [MessagePdfBlobId] IS NOT NULL
GO

CREATE NONCLUSTERED INDEX [IX_Messages_Rnu]
ON [dbo].[Messages]([Rnu])
GO
