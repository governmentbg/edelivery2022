GO

DECLARE
  @from DATE,
  @to DATE;
SET @from = '2023-10-10';
SET @to = GETDATE();

DECLARE
  @receivedTicketIndividuals INT,
  @receivedPenalDecreeIndividuals INT,
  @receivedTicketLegalEntites INT,
  @receivedPenalDecreeLegalEntites INT;

DECLARE
  @internalServed INT,
  @externalServed INT,
  @annulled INT;

DECLARE
  @emailNotifications INT,
  @phoneNotifications INT;

DECLARE
  @deliveredTicketIndividuals INT,
  @deliveredPenalDecreeIndividuals INT,
  @deliveredTicketLegalEntites INT,
  @deliveredPenalDecreeLegalEntites INT;

DECLARE
  @sentActiveProfiles INT,
  @sentPassiveProfiles INT;

WHILE @from < @to
BEGIN
  EXEC [dbo].[spGetTicketStats]
    @from,
    @receivedTicketIndividuals OUTPUT,
    @receivedPenalDecreeIndividuals OUTPUT,
    @receivedTicketLegalEntites OUTPUT,
    @receivedPenalDecreeLegalEntites OUTPUT,

    @internalServed OUTPUT,
    @externalServed OUTPUT,
    @annulled OUTPUT,

    @emailNotifications OUTPUT,
    @phoneNotifications OUTPUT,

    @deliveredTicketIndividuals OUTPUT,
    @deliveredPenalDecreeIndividuals OUTPUT,
    @deliveredTicketLegalEntites OUTPUT,
    @deliveredPenalDecreeLegalEntites OUTPUT,

    @sentActiveProfiles OUTPUT,
    @sentPassiveProfiles OUTPUT;

  INSERT INTO [reports].[TicketStats] VALUES(
    @from,
    @receivedTicketIndividuals,
    @receivedPenalDecreeIndividuals,
    @receivedTicketLegalEntites,
    @receivedPenalDecreeLegalEntites,
    @internalServed,
    @externalServed,
    @annulled,
    @emailNotifications,
    @phoneNotifications,
    @deliveredTicketIndividuals,
    @deliveredPenalDecreeIndividuals,
    @deliveredTicketLegalEntites,
    @deliveredPenalDecreeLegalEntites,
    @sentActiveProfiles,
    @sentPassiveProfiles);

  SET @from = DATEADD(day, 1, @from);
END

GO
