GO
PRINT 'AdminUsers'
GO

CREATE TABLE [dbo].[AdminUsers](
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [AccessFailedCount] [int] NOT NULL,
  [ConcurrencyStamp] [nvarchar](max) NULL,
  [Email] [nvarchar](256) NULL,
  [EmailConfirmed] [bit] NOT NULL,
  [LockoutEnabled] [bit] NOT NULL,
  [LockoutEnd] [datetimeoffset](7) NULL,
  [NormalizedEmail] [nvarchar](256) NULL,
  [NormalizedUserName] [nvarchar](256) NULL,
  [PasswordHash] [nvarchar](max) NULL,
  [PhoneNumber] [nvarchar](max) NULL,
  [PhoneNumberConfirmed] [bit] NOT NULL,
  [SecurityStamp] [nvarchar](max) NULL,
  [TwoFactorEnabled] [bit] NOT NULL,
  [UserName] [nvarchar](256) NULL,
  
  CONSTRAINT [PK_AdminUsers] PRIMARY KEY ([Id]),
)
GO
