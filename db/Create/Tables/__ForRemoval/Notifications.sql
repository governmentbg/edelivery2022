GO
PRINT 'Notifications'
GO

CREATE TABLE [dbo].[Notifications](
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [Type] [tinyint] NOT NULL,
  [Recipient] [nvarchar](255) NULL,
  [DateSent] [datetime] NOT NULL,

  CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
)
GO
