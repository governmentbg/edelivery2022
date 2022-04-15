GO
PRINT 'AdminUserTokens'
GO

CREATE TABLE [dbo].[AdminUserTokens](
  [UserId] [int] NOT NULL,
  [LoginProvider] [nvarchar](450) NOT NULL,
  [Name] [nvarchar](450) NOT NULL,
  [Value] [nvarchar](max) NULL,

  CONSTRAINT [PK_AdminUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
  CONSTRAINT [FK_AdminUserTokens_AdminUsers_UserId]
    FOREIGN KEY([UserId])
    REFERENCES [dbo].[AdminUsers] ([Id])
    ON DELETE CASCADE,
)
GO
