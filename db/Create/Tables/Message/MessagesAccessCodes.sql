GO
PRINT 'MessagesAccessCodes'
GO

CREATE TABLE [dbo].[MessagesAccessCodes](
  [MessageId] [int] NOT NULL,
  [AccessCode] [uniqueidentifier] NOT NULL,
  [ReceiverFirstName] [nvarchar](50) NOT NULL,
  [ReceiverMiddleName] [nvarchar](50) NULL,
  [ReceiverLastName] [nvarchar](50) NOT NULL,
  [ReceiverPhone] [nvarchar](50) NOT NULL,
  [ReceiverEmail] [nvarchar](50) NOT NULL,
  
  CONSTRAINT [PK_MessagesAccessCodes] PRIMARY KEY ([MessageId]),
  CONSTRAINT [UQ_MessagesAccessCodes_AccessCode] UNIQUE ([AccessCode]),
  CONSTRAINT [FK_MessageAccessCodes_Messages]
    FOREIGN KEY([MessageId])
    REFERENCES [dbo].[Messages] ([MessageId]),
)
GO
