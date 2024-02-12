GO
PRINT 'EmailDelivery'
GO

CREATE TABLE [reports].[EmailDelivery](
  [EmailDeliveryId] [bigint] IDENTITY(1,1) NOT NULL,
  [SentDate] [datetime2] NOT NULL,
  [Status] [tinyint] NOT NULL,
  [Tag] [nvarchar](256) NULL,
  CONSTRAINT [PK_EmailDelivery] PRIMARY KEY ([SentDate], [EmailDeliveryId])
)
ON psEmailDelivery ([SentDate]);
GO
