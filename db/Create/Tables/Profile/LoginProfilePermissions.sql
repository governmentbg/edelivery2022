GO
PRINT 'LoginProfilePermissions'
GO

CREATE TABLE [dbo].[LoginProfilePermissions](
  [LoginId] [int] NOT NULL,
  [ProfileId] [int] NOT NULL,
  [LoginProfilePermissionId] [int] IDENTITY(1,1) NOT NULL,
  [Permission] [int] NOT NULL,
  [TemplateId] [int] NULL,
  
  CONSTRAINT [PK_LoginProfilePermissions] PRIMARY KEY ([LoginId], [ProfileId], [LoginProfilePermissionId]),
  CONSTRAINT [FK_LoginProfilePermissions_LoginsProfiles]
    FOREIGN KEY([LoginId], [ProfileId])
    REFERENCES [dbo].[LoginsProfiles] ([LoginId], [ProfileId]),
  CONSTRAINT [FK_LoginProfilePermissions_Templates]
    FOREIGN KEY([TemplateId])
    REFERENCES [dbo].[Templates] ([TemplateId]),
  CONSTRAINT [CHK_LoginProfilePermissions_Permission]
      CHECK ([Permission] IN (
          1, -- OwnerAccess
          2, -- FullMessageAccess
          3, -- ReadProfileMessageAccess
          4, -- WriteProfileMessageAccess
          5, -- ListProfileMessageAccess
          6  -- AdministerProfileAccess
      )),
)
GO
