GO
PRINT 'ForwardedMessages'
GO

CREATE TABLE [dbo].[ForwardedMessages](
  [MessageId] [int] NOT NULL,
  [ForwardedMessageId] [int] NOT NULL,
  [ForwardedMessageSubject] [nvarchar](512) NOT NULL,
  [ForwardedMessageSenderProfileId] [int] NOT NULL,
  [ForwardedMessageSenderProfileName] [nvarchar](255) NOT NULL,
  [ForwardedMessageSenderProfileSubjectId] [uniqueidentifier] NULL,
  [ForwardedMessageSenderLoginName] [nvarchar](255) NULL,
  
  CONSTRAINT [PK_ForwardedMessages] PRIMARY KEY ([MessageId]),
  CONSTRAINT [FK_ForwardedMessages_Messages_ForwardedMessageId]
    FOREIGN KEY([ForwardedMessageId])
    REFERENCES [dbo].[Messages] ([MessageId]),
  CONSTRAINT [FK_ForwardedMessages_Messages_MessageId]
    FOREIGN KEY([MessageId])
    REFERENCES [dbo].[Messages] ([MessageId]),
  CONSTRAINT [FK_ForwardedMessages_Profiles]
    FOREIGN KEY([ForwardedMessageSenderProfileId])
    REFERENCES [dbo].[Profiles] ([Id]),
)
GO

CREATE NONCLUSTERED INDEX [IX_ForwardedMessages_ForwardedMessageId_FAC1F]
ON [dbo].[ForwardedMessages]([ForwardedMessageId])
GO
