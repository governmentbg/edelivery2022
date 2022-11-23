GO
PRINT 'ProfileBlobAccessKeys'
GO

CREATE TABLE [dbo].[ProfileBlobAccessKeys](
  [ProfileId] [int] NOT NULL,
  [BlobId] [int] NOT NULL,
  [ProfileKeyId] [int] NOT NULL,
  [CreatedByLoginId] [int] NULL,
  [EncryptedKey] [binary](256) NOT NULL,
  [Type] [int] NOT NULL,
  [Description] [nvarchar](500) NULL,
  [CreatedByAdminUserId] [int] NULL,
  
  CONSTRAINT [PK_ProfileBlobAccessKeys] PRIMARY KEY ([ProfileId], [BlobId]),
  CONSTRAINT [FK_ProfileBlobAccessKeys_AdminUsers_CreatedByAdminUserId]
    FOREIGN KEY([CreatedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_ProfileBlobAccessKeys_Blobs]
    FOREIGN KEY([BlobId])
    REFERENCES [dbo].[Blobs] ([BlobId]),
  CONSTRAINT [FK_ProfileBlobAccessKeys_Logins]
    FOREIGN KEY([CreatedByLoginId])
    REFERENCES [dbo].[Logins] ([Id]),
  CONSTRAINT [FK_ProfileBlobAccessKeys_ProfileKeys]
    FOREIGN KEY([ProfileKeyId])
    REFERENCES [dbo].[ProfileKeys] ([ProfileKeyId]),
  CONSTRAINT [FK_ProfileBlobAccessKeys_Profiles]
    FOREIGN KEY([ProfileId])
    REFERENCES [dbo].[Profiles] ([Id]),
  CONSTRAINT [CHK_ProfileBlobAccessKeys_Type] CHECK (
  [Type] IN (
        0, -- Temporary
        1, -- Storage
        2, -- Registration
        3, -- Template
        4,  -- PdfStamp
        10 -- Unknown
    ) 
    -- Storage Type is applicable only for non-system profiles
    AND ([Type] NOT IN (1) /*Storage*/ OR [ProfileId] != 1 /*SystemProfileId*/)
    -- Template Type is applicable only for system profiles
    AND ([Type] != 3 /*Template*/ OR [ProfileId] = 1 /*SystemProfileId*/)
  ),
)
GO

CREATE NONCLUSTERED INDEX [IX_ProfileBlobAccessKeys_BlobId]
ON [dbo].[ProfileBlobAccessKeys] ([BlobId])
GO
