GO
PRINT 'AdminRoleClaims'
GO

CREATE TABLE [dbo].[AdminRoleClaims](
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [ClaimType] [nvarchar](max) NULL,
  [ClaimValue] [nvarchar](max) NULL,
  [RoleId] [int] NOT NULL,

  CONSTRAINT [PK_AdminRoleClaims] PRIMARY KEY ([Id]),
  CONSTRAINT [FK_AdminRoleClaims_AdminRoles_RoleId]
    FOREIGN KEY([RoleId])
    REFERENCES [dbo].[AdminRoles] ([Id])
    ON DELETE CASCADE
)
GO
