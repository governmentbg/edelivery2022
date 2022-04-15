GO
PRINT 'AdminUserLogins'
GO

CREATE TABLE [dbo].[AdminUserLogins](
  [LoginProvider] [nvarchar](450) NOT NULL,
  [ProviderKey] [nvarchar](450) NOT NULL,
  [ProviderDisplayName] [nvarchar](max) NULL,
  [UserId] [int] NOT NULL,

  CONSTRAINT [PK_AdminUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
  CONSTRAINT [FK_AdminUserLogins_AdminUsers_UserId]
    FOREIGN KEY([UserId])
    REFERENCES [dbo].[AdminUsers] ([Id])
    ON DELETE CASCADE,
)
GO
