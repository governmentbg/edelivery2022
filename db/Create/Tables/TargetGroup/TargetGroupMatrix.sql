GO
PRINT 'TargetGroupMatrix'
GO

CREATE TABLE [dbo].[TargetGroupMatrix](
  [TargetGroupMatrixId] [int] IDENTITY(1,1) NOT NULL,
  [SenderTargetGroupId] [int] NOT NULL,
  [RecipientTargetGroupId] [int] NOT NULL,
  
  CONSTRAINT [PK_TargetGroupMatrix] PRIMARY KEY ([TargetGroupMatrixId]),
  CONSTRAINT [FK_TargetGroupMatrix_TargetGroup_1]
    FOREIGN KEY([SenderTargetGroupId])
    REFERENCES [dbo].[TargetGroups] ([TargetGroupId]),
  CONSTRAINT [FK_TargetGroupMatrix_TargetGroup_2]
    FOREIGN KEY([RecipientTargetGroupId])
    REFERENCES [dbo].[TargetGroups] ([TargetGroupId]),
)
GO
