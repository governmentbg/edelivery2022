GO
PRINT 'LoginsProfiles'
GO

CREATE TABLE [dbo].[LoginsProfiles](
  [LoginId] [int] NOT NULL,
  [ProfileId] [int] NOT NULL,
  [IsDefault] [bit] NOT NULL
    CONSTRAINT [DF_LoginsProfiles_IsDefault] DEFAULT (0),
  [EmailNotificationActive] [bit] NOT NULL
    CONSTRAINT [DF_LoginsProfiles_EmailNotificationActive] DEFAULT (1),
  [AccessGrantedBy] [int] NOT NULL,
  [DateAccessGranted] [datetime] NOT NULL,
  [EmailNotificationOnDeliveryActive] [bit] NOT NULL DEFAULT (0),
  [Email] [nvarchar](100) NOT NULL,
  [Phone] [nvarchar](50) NOT NULL,
  [AccessGrantedByAdminUserId] [int] NULL,
  [PhoneNotificationActive] [bit] NOT NULL
    CONSTRAINT [DF_LoginsProfiles_PhoneNotificationActive] DEFAULT (0),
  [PhoneNotificationOnDeliveryActive] [bit] NOT NULL
    CONSTRAINT [DF_LoginsProfiles_PhoneNotificationOnDeliveryActive] DEFAULT (0)
  
  CONSTRAINT [PK_LoginsProfiles] PRIMARY KEY ([LoginId], [ProfileId]),
  CONSTRAINT [FK_LoginsProfiles_AdminUsers_AccessGrantedByAdminUserId]
    FOREIGN KEY([AccessGrantedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_LoginsProfiles_LoginGrantedAccess]
    FOREIGN KEY([AccessGrantedBy])
    REFERENCES [dbo].[Logins] ([Id]),
  CONSTRAINT [FK_LoginsProfiles_Logins]
    FOREIGN KEY([LoginId])
    REFERENCES [dbo].[Logins] ([Id]),
  CONSTRAINT [FK_LoginsProfiles_Profiles]
    FOREIGN KEY([ProfileId])
    REFERENCES [dbo].[Profiles] ([Id]),
)
GO
