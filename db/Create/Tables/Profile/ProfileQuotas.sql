GO
PRINT 'ProfileQuotas'
GO

CREATE TABLE [dbo].[ProfileQuotas] (
    [ProfileId]               INT                 NOT NULL,
    [StorageQuotaInMb]        INT                 NULL,
    [DateModified]            DATETIME            NOT NULL,
    [ModifiedByAdminUserId]   INT                 NULL,

    CONSTRAINT [PK_ProfileQuotas] PRIMARY KEY ([ProfileId]),
    CONSTRAINT [FK_ProfileQuotas_Profiles] FOREIGN KEY ([ProfileId]) REFERENCES [dbo].[Profiles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProfileQuotas_AdminUsers_ModifiedByAdminUserId] FOREIGN KEY([ModifiedByAdminUserId]) REFERENCES [dbo].[AdminUsers] ([Id]),
)
GO
