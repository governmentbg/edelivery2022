GO
PRINT 'TicketStats'
GO

CREATE TABLE [reports].[TicketStats](
  [TicketStatDate] DATE NOT NULL,

  [ReceivedTicketIndividuals] [int] NOT NULL,
  [ReceivedPenalDecreeIndividuals] [int] NOT NULL,
  [ReceivedTicketLegalEntites] [int] NOT NULL,
  [ReceivedPenalDecreeLegalEntites] [int] NOT NULL,

  [InternalServed] [int] NOT NULL,
  [ExternalServed] [int] NOT NULL,
  [Annulled] [int] NOT NULL,

  [EmailNotifications] [int] NOT NULL,
  [PhoneNotifications] [int] NOT NULL,

  [DeliveredTicketIndividuals] [int] NOT NULL,
  [DeliveredPenalDecreeIndividuals] [int] NOT NULL,
  [DeliveredTicketLegalEntites] [int] NOT NULL,
  [DeliveredPenalDecreeLegalEntites] [int] NOT NULL,

  [SentToActiveProfiles] [int] NOT NULL,
  [SentToPassiveProfiles] [int] NOT NULL,
  CONSTRAINT [PK_TicketStats] PRIMARY KEY ([TicketStatDate])
)
GO
