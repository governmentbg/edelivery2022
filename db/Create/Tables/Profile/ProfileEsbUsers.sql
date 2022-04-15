GO
PRINT 'ProfileEsbUsers'
GO

CREATE TABLE [dbo].[ProfileEsbUsers] (
  [ProfileId]               INT                 NOT NULL,
  [OId]                     NVARCHAR(64)        NULL,
  [ClientId]                NVARCHAR(128)       NULL,
  [DateModified]            DATETIME            NOT NULL,
  [ModifiedByAdminUserId]   INT                 NULL,

  CONSTRAINT [PK_ProfileEsbUsers] PRIMARY KEY ([ProfileId]),
  CONSTRAINT [FK_ProfileEsbUsers_Profiles_ProfileId] FOREIGN KEY ([ProfileId]) REFERENCES [dbo].[Profiles] ([Id]) ON DELETE CASCADE,
  CONSTRAINT [FK_ProfileEsbUsers_AdminUsers_ModifiedByAdminUserId] FOREIGN KEY([ModifiedByAdminUserId]) REFERENCES [dbo].[AdminUsers] ([Id]),
)
GO
