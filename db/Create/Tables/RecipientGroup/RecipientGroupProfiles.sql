GO
PRINT 'RecipientGroupProfiles'
GO

CREATE TABLE [dbo].[RecipientGroupProfiles](
  [RecipientGroupId] [int] NOT NULL,
  [ProfileId] [int] NOT NULL,
  
  CONSTRAINT [PK_RecipientGroupProfiles] PRIMARY KEY ([RecipientGroupId], [ProfileId]),
  CONSTRAINT [FK_RecipientGroupProfiles_Profiles]
    FOREIGN KEY([ProfileId])
    REFERENCES [dbo].[Profiles] ([Id]),
  CONSTRAINT [FK_RecipientGroupProfiles_RecipientGroup]
    FOREIGN KEY([RecipientGroupId])
    REFERENCES [dbo].[RecipientGroups] ([RecipientGroupId]),
)
GO
