GO
PRINT 'Tickets'
GO

CREATE TABLE [dbo].[Tickets](
  [MessageId] [int] NOT NULL,
  [Type] [nvarchar](512) NOT NULL,
  [ViolationDate] [datetime2] NOT NULL,
  [SenderProfileId] [int] NULL,
  [DocumentSeries] [nvarchar](8) NULL,
  [DocumentNumber] [nvarchar](64) NULL,
  [IssueDate] [date] NULL,
  [RecipientIdentifier] [nvarchar](50) NULL,
  CONSTRAINT [PK_Tickets] PRIMARY KEY ([MessageId]),
  CONSTRAINT [FK_Tickets_Messages_MessageId] FOREIGN KEY([MessageId]) REFERENCES [dbo].[Messages] ([MessageId]),
);
GO

CREATE UNIQUE INDEX [UQ_Tickets_SenderProfileId_DocumentSeries_DocumentNumber_IssueDate]
ON [dbo].[Tickets]([SenderProfileId], [DocumentSeries], [DocumentNumber], [IssueDate], [RecipientIdentifier])
WHERE [DocumentNumber] IS NOT NULL
  AND [IssueDate] IS NOT NULL
  AND [SenderProfileId] IS NOT NULL
  AND [RecipientIdentifier] IS NOT NULL
GO
