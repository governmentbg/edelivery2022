GO
PRINT 'Profiles'
GO

CREATE TABLE [dbo].[Profiles](
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [IsActivated] [bit] NOT NULL
    CONSTRAINT [DF_Profiles_IsActivated] DEFAULT (0),
  [ProfileType] [int] NOT NULL,
  [ElectronicSubjectId] [uniqueidentifier] NOT NULL,
  [ElectronicSubjectName] [nvarchar](512) NOT NULL,
  [EmailAddress] [nvarchar](100) NOT NULL,
  [Phone] [nvarchar](50) NOT NULL,
  [DateCreated] [datetime] NOT NULL,
  [CreatedBy] [int] NOT NULL,
  [DateDeleted] [datetime] NULL,
  [DeletedByAdminUserId] [int] NULL,
  [Identifier] [nvarchar](50) NOT NULL,
  [IsReadOnly] [bit] NOT NULL,
  [IsPassive] [bit] NOT NULL,
  [EnableMessagesWithCode] [bit] NULL,
  [ModifyDate] [datetime2](7) NULL,
  [ModifiedBy] [int] NULL,
  [ActivatedDate] [datetime2](7) NULL,
  [AddressId] [int] NULL,
  [ActivatedByAdminUserId] [int] NULL,
  [CreatedByAdminUserId] [int] NULL,
  [ModifiedByAdminUserId] [int] NULL,
  
  CONSTRAINT [PK_Profiles] PRIMARY KEY ([Id]),
  CONSTRAINT [UC_Profiles_ElectronicSubjectId] UNIQUE ([ElectronicSubjectId]),
  CONSTRAINT [FK_Profiles_Addresses]
    FOREIGN KEY([AddressId])
    REFERENCES [dbo].[Addresses] ([AddressId]),
  CONSTRAINT [FK_Profiles_AdminUsers_ActivatedByAdminUserId]
    FOREIGN KEY([ActivatedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_Profiles_AdminUsers_CreatedByAdminUserId]
    FOREIGN KEY([CreatedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_Profiles_AdminUsers_DeletedByAdminUserId]
    FOREIGN KEY([DeletedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_Profiles_AdminUsers_ModifiedByAdminUserId]
    FOREIGN KEY([ModifiedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_Profiles_CreatedByLogin]
    FOREIGN KEY([CreatedBy])
    REFERENCES [dbo].[Logins] ([Id]),
  CONSTRAINT [FK_Profiles_Logins_ModifiedBy]
    FOREIGN KEY([ModifiedBy])
    REFERENCES [dbo].[Logins] ([Id]),
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Profiles]
ON [dbo].[Profiles]([ElectronicSubjectId])
GO

CREATE NONCLUSTERED INDEX [IX_Profiles_ElectronicSubjectName_527E0]
ON [dbo].[Profiles]([ElectronicSubjectName])
INCLUDE([Id])
GO
