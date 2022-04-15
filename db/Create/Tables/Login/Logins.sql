GO
PRINT 'Logins'
GO

CREATE TABLE [dbo].[Logins](
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [UserName] [nvarchar](100) NOT NULL,
  [Email] [nvarchar](100) NULL,
  [EmailConfirmed] [bit] NOT NULL
    CONSTRAINT [DF_Logins_EmailConfirmed] DEFAULT (1),
  [PasswordHash] [nvarchar](1024) NULL,
  [SecurityStamp] [nvarchar](1024) NULL,
  [PhoneNumber] [nvarchar](50) NULL,
  [PhoneNumberConfirmed] [bit] NOT NULL
    CONSTRAINT [DF_Logins_PhoneNumberConfirmed] DEFAULT (1),
  [TwoFactorEnabled] [bit] NOT NULL,
  [LockoutEndDateUtc] [datetime] NULL,
  [LockoutEnabled] [bit] NOT NULL,
  [AccessFailedCount] [int] NOT NULL
    CONSTRAINT [DF_Logins_AccessFailedCount] DEFAULT (0),
  [ElectronicSubjectId] [uniqueidentifier] NOT NULL,
  [ElectronicSubjectName] [nvarchar](255) NOT NULL,
  [IsActive] [bit] NOT NULL
    CONSTRAINT [DF_Logins_IsActive] DEFAULT (0),
  [CertificateThumbprint] [nvarchar](50) NULL,
  [CanSendOnBehalfOf] [bit] NULL DEFAULT (0),
  [PushNotificationsUrl] [nvarchar](500) NULL,

  CONSTRAINT [PK_EDeliveryUsers] PRIMARY KEY ([Id]),
)
GO

CREATE NONCLUSTERED INDEX [IX_Logins]
ON [dbo].[Logins]([ElectronicSubjectId])
GO

CREATE NONCLUSTERED INDEX [IX_Logins_IsActive_CertificateThumbprint]
ON [dbo].[Logins]([CertificateThumbprint])
WHERE
  [IsActive] = 1 
  AND [CertificateThumbprint] IS NOT NULL
GO

CREATE NONCLUSTERED INDEX [IX_Logins_Thumbprint]
ON [dbo].[Logins]([CertificateThumbprint])
GO

CREATE NONCLUSTERED INDEX [IX_Logins_UserName]
ON [dbo].[Logins]([UserName])
GO
