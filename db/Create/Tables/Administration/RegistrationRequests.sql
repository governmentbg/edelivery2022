GO
PRINT 'RegistrationRequests'
GO

CREATE TABLE [dbo].[RegistrationRequests](
  [RegistrationRequestId] [int] IDENTITY(1,1) NOT NULL,
  [RegisteredProfileId] [int] NOT NULL,
  [RegistrationEmail] [nvarchar](50) NOT NULL,
  [RegistrationPhone] [nvarchar](50) NOT NULL,
  [CreateDate] [datetime2](7) NOT NULL,
  [CreatedBy] [int] NULL,
  [ProcessDate] [datetime2](7) NULL,
  [ProcessedByAdminUserId] [int] NULL,
  [Status] [int] NOT NULL,
  [Comment] [nvarchar](max) NULL,
  [BlobId] [int] NOT NULL,
  
  [LegacyProcessByEGN] [nvarchar](50) NULL,

  CONSTRAINT [PK_RegistrationRequests] PRIMARY KEY ([RegistrationRequestId]),
  CONSTRAINT [FK_RegistrationRequests_AdminUsers_ProcessedByAdminUserId]
    FOREIGN KEY([ProcessedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_RegistrationRequests_Blobs_BlobId]
    FOREIGN KEY([BlobId])
    REFERENCES [dbo].[Blobs] ([BlobId]),
  CONSTRAINT [FK_RegistrationRequests_Logins_CreatedBy]
    FOREIGN KEY([CreatedBy])
    REFERENCES [dbo].[Logins] ([Id]),
  CONSTRAINT [FK_RegistrationRequests_Profiles_RegisteredProfileId]
    FOREIGN KEY([RegisteredProfileId])
    REFERENCES [dbo].[Profiles] ([Id]),
  CONSTRAINT [CHK_RegistrationRequests_Status]
    CHECK ([Status] IN (
        1, -- New
        2, -- Confirmed
        3, -- Rejected
        4  -- Deleted
    )),
)
GO
