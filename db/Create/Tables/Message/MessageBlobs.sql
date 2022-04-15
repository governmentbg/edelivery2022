GO
PRINT 'MessageBlobs'
GO

CREATE TABLE [dbo].[MessageBlobs](
  [MessageBlobId] [int] IDENTITY(1,1) NOT NULL,
  [MessageId] [int] NOT NULL,
  [BlobId] [int] NOT NULL,
  
  CONSTRAINT [PK_MessageBlobs] PRIMARY KEY NONCLUSTERED ([MessageBlobId]),
  CONSTRAINT [UQ_MessageBlobs] UNIQUE CLUSTERED ([MessageId], [MessageBlobId]),
  CONSTRAINT [FK_MessageBlobs_Blobs]
    FOREIGN KEY([BlobId])
    REFERENCES [dbo].[Blobs] ([BlobId]),
  CONSTRAINT [FK_MessageBlobs_Messages]
    FOREIGN KEY([MessageId])
    REFERENCES [dbo].[Messages] ([MessageId]),
)
GO

CREATE NONCLUSTERED INDEX [IX_MessageBlobs_BlobId]
ON [dbo].[MessageBlobs] ([BlobId])
GO
