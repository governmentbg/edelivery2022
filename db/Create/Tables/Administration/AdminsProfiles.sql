GO
PRINT 'AdminsProfiles'
GO

CREATE TABLE [dbo].[AdminsProfiles](
  [Id] [int] NOT NULL,
  [FirstName] [nvarchar](50) NOT NULL,
  [MiddleName] [nvarchar](50) NOT NULL,
  [LastName] [nvarchar](50) NOT NULL,
  [CreatedOn] [datetime] NOT NULL,
  [CreatedByAdminUserId] [int] NOT NULL,
  [EGN] [nchar](10) NOT NULL,
  [DisabledOn] [datetime] NULL,
  [DisabledByAdminUserId] [int] NULL,
  [DisableReason] [nvarchar](2048) NULL,
  
  CONSTRAINT [PK_AdminsProfiles] PRIMARY KEY ([Id]),
  CONSTRAINT [FK_AdminsProfiles_AdminUsers]
    FOREIGN KEY([Id])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_AdminsProfiles_AdminUsers_CreatedByAdminUserId]
    FOREIGN KEY([CreatedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_AdminsProfiles_AdminUsers_DisabledByAdminUserId]
    FOREIGN KEY([DisabledByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
)
GO
