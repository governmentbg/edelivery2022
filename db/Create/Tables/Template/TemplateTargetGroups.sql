GO
PRINT 'TemplateTargetGroups'
GO

CREATE TABLE [dbo].[TemplateTargetGroups](
  [TemplateId] [int] NOT NULL,
  [TargetGroupId] [int] NOT NULL,
  [CanSend] [bit] NOT NULL,
  [CanReceive] [bit] NOT NULL,
  
  CONSTRAINT [PK_TemplateTargetGroups] PRIMARY KEY ([TemplateId], [TargetGroupId]),
  CONSTRAINT [FK_TemplateTargetGroups_TargetGroups]
    FOREIGN KEY([TargetGroupId])
    REFERENCES [dbo].[TargetGroups] ([TargetGroupId]),
  CONSTRAINT [FK_TemplateTargetGroups_Templates]
    FOREIGN KEY([TemplateId])
    REFERENCES [dbo].[Templates] ([TemplateId]),
)
GO
