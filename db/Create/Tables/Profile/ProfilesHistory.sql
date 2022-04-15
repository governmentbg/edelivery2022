GO
PRINT 'ProfilesHistory'
GO

CREATE TABLE [dbo].[ProfilesHistory](
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [ProfileId] [int] NOT NULL,
  [Action] [nvarchar](255) NOT NULL,
  [ActionLogin] [int] NULL,
  [ActionDate] [datetime] NOT NULL,
  [ActionDetails] [xml] NULL,
  [IPAddress] [nvarchar](50) NULL,
  [ActionByAdminUserId] [int] NULL,
  
  CONSTRAINT [PK_ProfilesHistory] PRIMARY KEY ([Id]),
  CONSTRAINT [FK_ProfilesHistory_ActionMadeByLogin]
    FOREIGN KEY([ActionLogin])
    REFERENCES [dbo].[Logins] ([Id]),
  CONSTRAINT [FK_ProfilesHistory_AdminUsers_ActionByAdminUserId]
    FOREIGN KEY([ActionByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_ProfilesHistory_Profiles]
    FOREIGN KEY([ProfileId])
    REFERENCES [dbo].[Profiles] ([Id]),
  
)
GO

CREATE NONCLUSTERED INDEX [IX_ProfilesHistory_ProfileId]
ON [dbo].[ProfilesHistory] ([ProfileId])
GO
