GO
PRINT 'ProfileKeys'
GO

CREATE TABLE [dbo].[ProfileKeys](
  [ProfileKeyId] [int] IDENTITY(1,1) NOT NULL,
  [ProfileId] [int] NOT NULL,
  [Provider] [nvarchar](100) NOT NULL,
  [KeyName] [nvarchar](100) NOT NULL,
  [OaepPadding] [nvarchar](20) NOT NULL,
  [IssuedAt] [datetime] NOT NULL,
  [ExpiresAt] [datetime] NOT NULL,
  [IsActive] [bit] NOT NULL,
  
  CONSTRAINT [PK_ProfileKeys] PRIMARY KEY ([ProfileKeyId]),
  CONSTRAINT [FK_ProfileKeys_Profiles]
    FOREIGN KEY([ProfileId])
    REFERENCES [dbo].[Profiles] ([Id]),
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ProfileKeys_IsActive]
ON [dbo].[ProfileKeys] ([ProfileId])
WHERE [IsActive] = 1 
GO
