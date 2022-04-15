GO
PRINT 'LoginsRoles'
GO

CREATE TABLE [dbo].[LoginsRoles](
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [UserId] [int] NOT NULL,
  [RoleId] [int] NOT NULL,
  
  CONSTRAINT [PK_LoginsRoles] PRIMARY KEY ([Id]),
  CONSTRAINT [FK_LoginsRoles_Logins]
    FOREIGN KEY([UserId])
    REFERENCES [dbo].[Logins] ([Id]),
  CONSTRAINT [FK_LoginsRoles_Roles]
    FOREIGN KEY([RoleId])
    REFERENCES [dbo].[Roles] ([Id]),
)
GO

CREATE NONCLUSTERED INDEX [IX_LoginsRoles_UserId]
ON [dbo].[LoginsRoles]([UserId])
GO
