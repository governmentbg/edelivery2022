GO
PRINT 'QueueMessages'
GO

CREATE SEQUENCE [dbo].[QueueMessagesIdSequence]
AS
    INT START WITH 1
    INCREMENT BY 1
    NO CYCLE
GO

CREATE TABLE [dbo].[QueueMessages](
  [Status] [int] NOT NULL,
  [Type] [int] NOT NULL,
  [DueDate] [bigint] NOT NULL,
  [QueueMessageId] [int] NOT NULL,
  [Key] [nvarchar](100) NULL,
  [Tag] [nvarchar](max) NULL,
  [Payload] [nvarchar](max) NULL,
  [FailedAttempts] [int] NOT NULL,
  [FailedAttemptsErrors] [nvarchar](max) NULL,
  [CreateDate] [datetime2](7) NOT NULL,
  [StatusDate] [datetime2](7) NOT NULL,
  [Version] [timestamp] NOT NULL,
  CONSTRAINT [PK_QueueMessages] PRIMARY KEY ([Status], [Type], [DueDate], [QueueMessageId]),
  CONSTRAINT [CHK_QueueMessages_Status] CHECK ([Status] IN (1, 2, 3, 4, 5)),
  CONSTRAINT [CHK_QueueMessages_Type] CHECK (
       ([Type]=1) -- EmailNotification
    OR ([Type]=2) -- SmsNotification
    OR ([Type]=3) -- PushNotification
    OR ([Type]=4) -- ViberNotification
    OR ([Type]=5) -- SmsDeliveryCheck
    OR ([Type]=6) -- ViberDeliveryCheck
    OR ([Type]=7) -- Translation
    OR ([Type]=8) -- TranslationClosure
    OR ([Type]=9) -- DeliveredTicket
    OR ([Type]=10) -- DataPortal
  ),
)
GO

CREATE UNIQUE INDEX [UQ_QueueMessages_Key]
ON [dbo].[QueueMessages]([Key])
WHERE [Key] IS NOT NULL
GO
