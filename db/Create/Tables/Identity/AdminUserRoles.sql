GO
PRINT 'AdminUserRoles'
GO

CREATE TABLE [dbo].[AdminUserRoles](
  [UserId] [int] NOT NULL,
  [RoleId] [int] NOT NULL,

  CONSTRAINT [PK_AdminUserRoles] PRIMARY KEY ([UserId], [RoleId]),
  CONSTRAINT [FK_AdminUserRoles_AdminRoles_RoleId]
    FOREIGN KEY([RoleId])
    REFERENCES [dbo].[AdminRoles] ([Id])
    ON DELETE CASCADE,
  CONSTRAINT [FK_AdminUserRoles_AdminUsers_UserId]
    FOREIGN KEY([UserId])
    REFERENCES [dbo].[AdminUsers] ([Id])
    ON DELETE CASCADE,
)
GO
