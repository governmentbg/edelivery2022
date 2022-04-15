GO
PRINT 'MessageAccessKeys'
GO

CREATE TABLE [dbo].[MessageAccessKeys](
  [ProfileId] [int] NOT NULL,
  [MessageId] [int] NOT NULL,
  [ProfileKeyId] [int] NOT NULL,
  [EncryptedKey] [binary](256) NOT NULL,
  
  CONSTRAINT [PK_MessageAccessKeys] PRIMARY KEY ([MessageId], [ProfileId]),
  CONSTRAINT [FK_MessageAccessKeys_Messages]
    FOREIGN KEY([MessageId])
    REFERENCES [dbo].[Messages] ([MessageId]),
  CONSTRAINT [FK_MessageAccessKeys_ProfileKeys]
    FOREIGN KEY([ProfileKeyId])
    REFERENCES [dbo].[ProfileKeys] ([ProfileKeyId]),
)
GO
