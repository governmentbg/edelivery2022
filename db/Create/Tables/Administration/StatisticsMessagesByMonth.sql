GO
PRINT 'StatisticsMessagesByMonth'
GO

CREATE TABLE [dbo].[StatisticsMessagesByMonth](
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [MonthDate] [date] NOT NULL,
  [Month] [nvarchar](50) NOT NULL,
  [MessagesSent] [int] NOT NULL,
  [MessagesReceived] [int] NOT NULL,
  
  CONSTRAINT [PK_StatisticsMessagesByMonth] PRIMARY KEY ([Id]),
)
GO
