GO
PRINT 'TemplateProfiles'
GO

CREATE TABLE [dbo].[TemplateProfiles](
  [TemplateId] [int] NOT NULL,
  [ProfileId] [int] NOT NULL,
  [CanSend] [bit] NOT NULL,
  [CanReceive] [bit] NOT NULL,
  
  CONSTRAINT [PK_TemplateProfiles] PRIMARY KEY ([TemplateId], [ProfileId]),
  CONSTRAINT [FK_TemplateProfiles_Profiles]
    FOREIGN KEY([ProfileId])
    REFERENCES [dbo].[Profiles] ([Id]),
  CONSTRAINT [FK_TemplateProfiles_Templates]
    FOREIGN KEY([TemplateId])
    REFERENCES [dbo].[Templates] ([TemplateId]),
)
GO
