GO
PRINT 'Templates'
GO

CREATE TABLE [dbo].[Templates](
  [TemplateId] [int] IDENTITY(1,1) NOT NULL,
  [Name] [nvarchar](500) NOT NULL,
  [IdentityNumber] [nvarchar](50) NOT NULL,
  [Content] [nvarchar](max) NOT NULL,
  [IsSystemTemplate] [bit] NOT NULL,
  [ReadLoginSecurityLevelId] [int] NOT NULL,
  [WriteLoginSecurityLevelId] [int] NOT NULL,
  [ResponseTemplateId] [int] NULL,
  [CreateDate] [datetime2](7) NOT NULL,
  [CreatedByAdminUserId] [int] NOT NULL,
  [ArchiveDate] [datetime2](7) NULL,
  [ArchivedByAdminUserId] [int] NULL,
  [PublishDate] [datetime2](7) NULL,
  [PublishedByAdminUserId] [int] NULL,
  [BlobId] [int] NULL,
  [SenderDocumentField] [nvarchar](512) NULL,
  [RecipientDocumentField] [nvarchar](512) NULL,
  [SubjectDocumentField] [nvarchar](512) NULL,
  [DateSentDocumentField] [nvarchar](512) NULL,
  [DateReceivedDocumentField] [nvarchar](512) NULL,
  [SenderSignatureDocumentField] [nvarchar](512) NULL,
  [RecipientSignatureDocumentField] [nvarchar](512) NULL,
  
  CONSTRAINT [PK_Templates] PRIMARY KEY ([TemplateId]),
  CONSTRAINT [FK_Templates_AdminUsers_ArchivedByAdminUserId]
    FOREIGN KEY([ArchivedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_Templates_AdminUsers_CreatedByAdminUserId]
    FOREIGN KEY([CreatedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_Templates_AdminUsers_PublishedByAdminUserId]
    FOREIGN KEY([PublishedByAdminUserId])
    REFERENCES [dbo].[AdminUsers] ([Id]),
  CONSTRAINT [FK_Templates_Blobs_BlobId]
    FOREIGN KEY([BlobId])
    REFERENCES [dbo].[Blobs] ([BlobId]),
  CONSTRAINT [FK_Templates_LoginSecurityLevels_1]
    FOREIGN KEY([ReadLoginSecurityLevelId])
    REFERENCES [dbo].[LoginSecurityLevels] ([LoginSecurityLevelId]),
  CONSTRAINT [FK_Templates_LoginSecurityLevels_2]
    FOREIGN KEY([WriteLoginSecurityLevelId])
    REFERENCES [dbo].[LoginSecurityLevels] ([LoginSecurityLevelId]),
  CONSTRAINT [FK_Templates_Templates]
    FOREIGN KEY([ResponseTemplateId])
    REFERENCES [dbo].[Templates] ([TemplateId]),
)
GO
