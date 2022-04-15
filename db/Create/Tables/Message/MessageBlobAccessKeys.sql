GO
PRINT 'MessageBlobAccessKeys'
GO

CREATE TABLE [dbo].[MessageBlobAccessKeys](
  [ProfileId] [int] NOT NULL,
  [MessageBlobId] [int] NOT NULL,
  [ProfileKeyId] [int] NOT NULL,
  [EncryptedKey] [binary](256) NOT NULL,
  
  CONSTRAINT [PK_MessageBlobAccessKeys] PRIMARY KEY ([MessageBlobId], [ProfileId]),
  CONSTRAINT [FK_MessageBlobAccessKeys_MessageBlobs]
    FOREIGN KEY([MessageBlobId])
    REFERENCES [dbo].[MessageBlobs] ([MessageBlobId]),
  CONSTRAINT [FK_MessageBlobAccessKeys_ProfileKeys]
    FOREIGN KEY([ProfileKeyId])
    REFERENCES [dbo].[ProfileKeys] ([ProfileKeyId]),
  CONSTRAINT [FK_MessageBlobAccessKeys_Profiles]
    FOREIGN KEY([ProfileId])
    REFERENCES [dbo].[Profiles] ([Id]),
)
GO
