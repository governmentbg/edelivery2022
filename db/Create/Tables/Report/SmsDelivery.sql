GO
PRINT 'SmsDelivery'
GO

CREATE TABLE [reports].[SmsDelivery](
  [SmsDeliveryId] [bigint] IDENTITY(1,1) NOT NULL,
  [SentDate] [datetime2] NOT NULL,
  [MsgId] [int] NOT NULL,
  [Status] [tinyint] NOT NULL,
  [Charge] [bit] NOT NULL,
  [Tag] [nvarchar](256) NULL,
  CONSTRAINT [PK_SmsDelivery] PRIMARY KEY ([SentDate], [SmsDeliveryId])
)
ON psSmsDelivery ([SentDate]);
GO
