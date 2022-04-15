GO
PRINT 'LoginsClaims'
GO

CREATE TABLE [dbo].[LoginsClaims](
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [UserId] [int] NOT NULL,
  [ClaimType] [nvarchar](100) NULL,
  [ClaimValue] [nvarchar](100) NULL,
  
  CONSTRAINT [PK_LoginsClaims] PRIMARY KEY ([Id]),
  CONSTRAINT [FK_LoginsClaims_Logins]
    FOREIGN KEY([UserId])
    REFERENCES [dbo].[Logins] ([Id]),
)
GO
