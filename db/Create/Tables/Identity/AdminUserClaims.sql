GO
PRINT 'AdminUserClaims'
GO

CREATE TABLE [dbo].[AdminUserClaims](
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [ClaimType] [nvarchar](max) NULL,
  [ClaimValue] [nvarchar](max) NULL,
  [UserId] [int] NOT NULL,

  CONSTRAINT [PK_AdminUserClaims] PRIMARY KEY ([Id]),
  CONSTRAINT [FK_AdminUserClaims_AdminUsers_UserId]
    FOREIGN KEY([UserId])
    REFERENCES [dbo].[AdminUsers] ([Id])
    ON DELETE CASCADE,
)
GO
