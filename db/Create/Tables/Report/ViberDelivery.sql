GO
PRINT 'ViberDelivery'
GO

CREATE TABLE [reports].[ViberDelivery](
  [ViberDeliveryId] [bigint] IDENTITY(1,1) NOT NULL,
  [SentDate] [datetime2] NOT NULL,
  [MsgId] [int] NOT NULL,
  [Status] [tinyint] NOT NULL,
  [Charge] [bit] NOT NULL,
  [Tag] [nvarchar](256) NULL,
  CONSTRAINT [PK_ViberDelivery] PRIMARY KEY ([SentDate], [ViberDeliveryId])
)
ON psViberDelivery ([SentDate]);
GO
