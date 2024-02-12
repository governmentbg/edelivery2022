GO
PRINT 'TicketDelivery'
GO

CREATE TABLE [reports].[TicketDelivery](
  [TicketDeliveryId] [bigint] IDENTITY(1,1) NOT NULL,
  [SentDate] [datetime2] NOT NULL,
  [MessageId] [int] NOT NULL,
  [Status] [tinyint] NOT NULL,
  CONSTRAINT [PK_TicketDelivery] PRIMARY KEY ([SentDate], [TicketDeliveryId])
)
ON psTicketDelivery ([SentDate]);
GO
