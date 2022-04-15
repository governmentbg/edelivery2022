GO
PRINT 'RecipientGroups'
GO

CREATE TABLE [dbo].[RecipientGroups](
  [RecipientGroupId] [int] IDENTITY(1,1) NOT NULL,
  [ProfileId] [int] NULL,
  [Name] [nvarchar](100) NOT NULL,
  [IsPublic] [bit] NOT NULL,
  [CreateDate] [datetime2](7) NOT NULL,
  [CreatedBy] [int] NOT NULL,
  [ModifyDate] [datetime2](7) NOT NULL,
  [ModifiedBy] [int] NOT NULL,
  [ArchiveDate] [datetime2](7) NULL,
  [ArchivedBy] [int] NULL,
  [CreatedByAdminUserId] [int] NULL,
  [ModifiedByAdminUserId] [int] NULL,
  [ArchivedByAdminUserId] [int] NULL,
  CONSTRAINT [PK_RecipientGroups] PRIMARY KEY ([RecipientGroupId]),
  CONSTRAINT [FK_RecipientGroups_AdminUsers_ArchivedByAdminUserId]
    FOREIGN KEY([ArchivedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_RecipientGroups_AdminUsers_CreatedByAdminUserId]
    FOREIGN KEY([CreatedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_RecipientGroups_AdminUsers_ModifiedByAdminUserId]
    FOREIGN KEY([ModifiedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_RecipientGroups_Profiles]
    FOREIGN KEY([ProfileId])
    REFERENCES [dbo].[Profiles] ([Id]),
  
)
GO
