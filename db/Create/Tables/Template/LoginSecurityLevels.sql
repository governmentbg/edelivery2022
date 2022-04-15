GO
PRINT 'LoginSecurityLevels'
GO

CREATE TABLE [dbo].[LoginSecurityLevels](
  [LoginSecurityLevelId] [int] IDENTITY(1,1) NOT NULL,
  [Name] [nvarchar](100) NOT NULL,
  [NumValue] [int] NOT NULL,
  [CreateDate] [datetime2](7) NOT NULL,
  [CreatedByAdminUserId] [int] NOT NULL,
  [ArchiveDate] [datetime2](7) NULL,
  [ArchivedByAdminUserId] [int] NULL,
  
  CONSTRAINT [PK_LoginSecurityLevels] PRIMARY KEY ([LoginSecurityLevelId]),
  CONSTRAINT [FK_LoginSecurityLevels_AdminUsers_ArchivedByAdminUserId]
    FOREIGN KEY([ArchivedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_LoginSecurityLevels_AdminUsers_CreatedByAdminUserId]
    FOREIGN KEY([CreatedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
)
GO
