GO
PRINT 'RegixReportsAuditLog'
GO

CREATE TABLE [dbo].[RegixReportsAuditLog](
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [Token] [uniqueidentifier] NOT NULL,
  [Data] [nvarchar](1000) NOT NULL,
  [CreatedByLoginId] [int] NOT NULL,
  [CreatedForProfileId] [int] NOT NULL,
  [DateCreated] [datetime] NOT NULL,
  
  CONSTRAINT [PK_RegixReportsAuditLog] PRIMARY KEY ([Id]),
)
GO
