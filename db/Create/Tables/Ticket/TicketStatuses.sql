GO
PRINT 'TicketStatuses'
GO

CREATE TABLE [dbo].[TicketStatuses](
  [MessageId] [int] NOT NULL,
  [TicketStatusId] [int] IDENTITY(1,1) NOT NULL,
  [Status] [int] NOT NULL,
  --begin specific props
  [ServeDate] [datetime2](7) NULL,
  [AnnulDate] [datetime2](7) NULL,
  [AnnulmentReason] [nvarchar](max) NULL,
  -- end specific props
  [CreateDate] [datetime2](7) NOT NULL,
  [CreatedByLoginId] [int] NOT NULL,
  [SeenDate] [datetime2](7) NULL,
  [SeenByLoginId] [int] NULL,
  CONSTRAINT [PK_TicketStatuses] PRIMARY KEY ([MessageId], [TicketStatusId]),
  CONSTRAINT [FK__TicketStatuses_Tickets_MessageId] FOREIGN KEY([MessageId]) REFERENCES [dbo].[Tickets] ([MessageId]),
  CONSTRAINT [FK_TicketStatuses_Logins_CreatedByLoginId] FOREIGN KEY ([CreatedByLoginId]) REFERENCES [dbo].[Logins] ([Id]),
  CONSTRAINT [FK_TicketStatuses_Logins_SeenByLoginId] FOREIGN KEY ([SeenByLoginId]) REFERENCES [dbo].[Logins] ([Id]),
  CONSTRAINT [CHK_TicketStatuses_Status] CHECK (
    [Status] IN (
          0, -- Не е връчен
          1, -- Връчен през ССЕВ
          2, -- Връчен извън ССЕВ
          99 -- Анулиран
      )
  ),
);
GO
