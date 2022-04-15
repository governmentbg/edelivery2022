GO
PRINT 'ExternalLogins'
GO

CREATE TABLE [dbo].[ExternalLogins](
  [UserId] [int] NOT NULL,
  [LoginProvider] [nvarchar](128) NOT NULL,
  [ProviderKey] [nvarchar](128) NOT NULL,
  
  CONSTRAINT [PK_ExternalLogins] PRIMARY KEY ([UserId], [LoginProvider], [ProviderKey]),
  CONSTRAINT [FK_ExternalLogins_Logins]
    FOREIGN KEY([UserId])
    REFERENCES [dbo].[Logins] ([Id]),
)
GO
