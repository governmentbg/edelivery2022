GO
PRINT 'MessageTranslations'
GO

CREATE TABLE [dbo].[MessageTranslations](
  [MessageTranslationId] [int] IDENTITY(1,1) NOT NULL,
  [MessageId] [int] NOT NULL,
  [ProfileId] [int] NOT NULL,
  [SourceLanguage] [nvarchar](4) NOT NULL,
  [TargetLanguage] [nvarchar](4) NOT NULL,
  [CreateDate] [datetime2] NOT NULL,
  [CreatedBy] [int] NOT NULL,
  [ModifyDate] [datetime2] NOT NULL,
  [ArchiveDate] [datetime2] NULL,
  [ArchivedBy] [int] NULL,
  CONSTRAINT [PK_MessageTranslations] PRIMARY KEY ([MessageTranslationId]),
  CONSTRAINT [FK_MessageTranslations_Messages_MessageId] FOREIGN KEY([MessageId]) REFERENCES [dbo].[Messages] ([MessageId]),
  CONSTRAINT [FK_MessageTranslations_Profiles_ProfileId] FOREIGN KEY([ProfileId]) REFERENCES [dbo].[Profiles] ([Id]),
  CONSTRAINT [FK_MessageTranslations_Logins_CreatedBy] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[Logins] ([Id]),
  CONSTRAINT [FK_MessageTranslations_Logins_ArchivedBy] FOREIGN KEY([ArchivedBy]) REFERENCES [dbo].[Logins] ([Id])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_MessageTranslations_MessageId_ProfileId_SourceLanguage_TargetLanguage]
ON [dbo].[MessageTranslations] ([MessageId], [ProfileId], [SourceLanguage], [TargetLanguage])
GO
