GO
PRINT 'TargetGroupProfiles'
GO

CREATE TABLE [dbo].[TargetGroupProfiles](
  [TargetGroupId] [int] NOT NULL,
  [ProfileId] [int] NOT NULL,
  
  CONSTRAINT [PK_TargetGroupProfiles] PRIMARY KEY ([TargetGroupId], [ProfileId]),
  CONSTRAINT [FK_TargetGroupProfiles_Profiles]
    FOREIGN KEY([ProfileId])
    REFERENCES [dbo].[Profiles] ([Id]),
  CONSTRAINT [FK_TargetGroupProfiles_TargetGroup]
    FOREIGN KEY([TargetGroupId])
    REFERENCES [dbo].[TargetGroups] ([TargetGroupId]),
)
GO

CREATE NONCLUSTERED INDEX [IX_TargetGroupProfiles_ProfileId]
ON [dbo].[TargetGroupProfiles] ([ProfileId])
GO
