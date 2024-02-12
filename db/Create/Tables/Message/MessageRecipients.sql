GO
PRINT 'MessageRecipients'
GO

CREATE TABLE [dbo].[MessageRecipients](
  [MessageId] [int] NOT NULL,
  [ProfileId] [int] NOT NULL,
  [LoginId] [int] NULL,
  [DateReceived] [datetime2](7) NULL,
  [Timestamp] [varbinary](8000) NULL,
  [MessagePdfBlobId] [int] NULL,
  [MessageSummaryXml] [xml] NULL,
  [MessageSummary] [varbinary](MAX) NULL,
  
  CONSTRAINT [PK_MessageRecipients] PRIMARY KEY ([MessageId], [ProfileId]),
  CONSTRAINT [FK_MessageRecipients_Blobs_MessagePdfBlobId]
    FOREIGN KEY([MessagePdfBlobId])
    REFERENCES [dbo].[Blobs] ([BlobId]),
  CONSTRAINT [FK_MessageRecipients_Messages]
    FOREIGN KEY([MessageId])
    REFERENCES [dbo].[Messages] ([MessageId]),
  CONSTRAINT [FK_MessageRecipients_Profiles]
    FOREIGN KEY([ProfileId])
    REFERENCES [dbo].[Profiles] ([Id]),
)
GO

CREATE NONCLUSTERED INDEX [IX_MessageRecipients_ProfileId]
ON [dbo].[MessageRecipients] ([ProfileId])
INCLUDE ([LoginId], [DateReceived])
GO

CREATE NONCLUSTERED INDEX [IX_MessageRecipients_MessagePdfBlobId]
ON [dbo].[MessageRecipients] ([MessagePdfBlobId])
WHERE [MessagePdfBlobId] IS NOT NULL
GO

CREATE NONCLUSTERED INDEX [IX_MessageRecipients_MessageId_ProfileId]
ON [dbo].[MessageRecipients] ([MessageId], [ProfileId])
INCLUDE ([LoginId], [DateReceived])
GO
