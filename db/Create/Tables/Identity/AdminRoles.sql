GO
PRINT 'AdminRoles'
GO

CREATE TABLE [dbo].[AdminRoles](
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [ConcurrencyStamp] [nvarchar](max) NULL,
  [Name] [nvarchar](256) NULL,
  [NormalizedName] [nvarchar](256) NULL,

  CONSTRAINT [PK_AdminRoles] PRIMARY KEY ([Id]),
)
GO
