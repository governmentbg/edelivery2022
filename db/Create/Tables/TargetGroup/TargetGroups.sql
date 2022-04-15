GO
PRINT 'TargetGroups'
GO

CREATE TABLE [dbo].[TargetGroups](
  [TargetGroupId] [int] IDENTITY(1,1) NOT NULL,
  [Name] [nvarchar](100) NOT NULL,
  [CreateDate] [datetime2](7) NOT NULL,
  [CreatedByAdminUserId] [int] NOT NULL,
  [ModifyDate] [datetime2](7) NOT NULL,
  [ModifiedByAdminUserId] [int] NOT NULL,
  [ArchiveDate] [datetime2](7) NULL,
  [ArchivedByAdminUserId] [int] NULL,
  
  CONSTRAINT [PK_TargetGroups] PRIMARY KEY ([TargetGroupId]),
  CONSTRAINT [FK_TargetGroups_AdminUsers_ArchivedByAdminUserId]
    FOREIGN KEY([ArchivedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_TargetGroups_AdminUsers_CreatedByAdminUserId]
    FOREIGN KEY([CreatedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_TargetGroups_AdminUsers_ModifiedByAdminUserId]
    FOREIGN KEY([ModifiedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
)
GO
