PRINT 'Create spGetTicketStats'
GO

CREATE OR ALTER PROCEDURE [dbo].[spGetTicketStats]
  @ticketStatDate DATE,

  @receivedTicketIndividuals INT OUTPUT,
  @receivedPenalDecreeIndividuals INT OUTPUT,
  @receivedTicketLegalEntites INT OUTPUT,
  @receivedPenalDecreeLegalEntites INT OUTPUT,

  @internalServed INT OUTPUT,
  @externalServed INT OUTPUT,
  @annulled INT OUTPUT,

  @emailNotifications INT OUTPUT,
  @phoneNotifications INT OUTPUT,

  @deliveredTicketIndividuals INT OUTPUT,
  @deliveredPenalDecreeIndividuals INT OUTPUT,
  @deliveredTicketLegalEntites INT OUTPUT,
  @deliveredPenalDecreeLegalEntites INT OUTPUT,

  @sentToActiveProfiles INT OUTPUT,
  @sentToPassiveProfiles INT OUTPUT

AS

  SET NOCOUNT ON;

  -- set filters
  DECLARE @to DATE, @from DATE;
  SET @from = @ticketStatDate;
  SET @to = DATEADD(DAY, 1, @from);

  --получени адм. актове (е-фишове)
  DECLARE
    @_type NVARCHAR(512),
    @_targetGroupId INT,
    @receivedTicketPublicAdministrations INT,
    @receivedPenalDecreePublicAdministrations INT,
    @receivedTicketSocialOrganizations INT,
    @receivedPenalDecreeSocialOrganizations INT;

  DECLARE received_cursor CURSOR FOR
    SELECT
      all_combinations.Type,
      all_combinations.TargetGroupId,
      COALESCE(aggregated_counts.MessageCount, 0) AS MessageCount
    FROM
      (SELECT DISTINCT t.Type, tgp.TargetGroupId
      FROM Tickets t
          CROSS JOIN TargetGroupProfiles tgp
      where tgp.TargetGroupId in (1,2,3,4)) AS all_combinations
      LEFT JOIN
        (SELECT
            t.Type,
            tgp.TargetGroupId,
            COUNT(m.MessageId) AS MessageCount
        FROM Tickets t
            JOIN Messages m ON t.MessageId = m.MessageId
            JOIN MessageRecipients mr ON m.MessageId = mr.MessageId
            JOIN Profiles rp ON mr.ProfileId = rp.Id
            JOIN TargetGroupProfiles tgp ON rp.Id = tgp.ProfileId
        WHERE m.DateSent >= @from AND m.DateSent < @to
        GROUP BY t.Type, tgp.TargetGroupId) AS aggregated_counts ON all_combinations.Type = aggregated_counts.Type AND all_combinations.TargetGroupId = aggregated_counts.TargetGroupId
    ORDER BY all_combinations.TargetGroupId, all_combinations.Type;

  -- individuals
  OPEN received_cursor;
  FETCH NEXT FROM received_cursor INTO @_type, @_targetGroupId, @receivedTicketIndividuals; -- !

  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM received_cursor INTO @_type, @_targetGroupId, @receivedPenalDecreeIndividuals; -- !

  -- legal entites
  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM received_cursor INTO @_type, @_targetGroupId, @receivedTicketLegalEntites; -- !

  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM received_cursor INTO @_type, @_targetGroupId, @receivedPenalDecreeLegalEntites; -- !

  -- public administrations
  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM received_cursor INTO @_type, @_targetGroupId, @receivedTicketPublicAdministrations;

  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM received_cursor INTO @_type, @_targetGroupId, @receivedPenalDecreePublicAdministrations;

  -- social organizations
  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM received_cursor INTO @_type, @_targetGroupId, @receivedTicketSocialOrganizations;

  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM received_cursor INTO @_type, @_targetGroupId, @receivedPenalDecreeSocialOrganizations;

  CLOSE received_cursor;
  DEALLOCATE received_cursor;

  SET @receivedTicketLegalEntites = @receivedTicketLegalEntites + @receivedTicketPublicAdministrations + @receivedTicketSocialOrganizations; -- !
  SET @receivedPenalDecreeLegalEntites = @receivedPenalDecreeLegalEntites + @receivedPenalDecreePublicAdministrations + @receivedPenalDecreeSocialOrganizations; -- !

  --разбивка по статус
  declare @_status INT;

  DECLARE status_cursor CURSOR FOR
    SELECT
      1 AS [Status],
      COUNT(t.MessageId)
    FROM Tickets t
    INNER JOIN MessageRecipients mr ON t.MessageId = mr.MessageId
    CROSS APPLY (
      SELECT TOP 1
          [ts].[Status],
          [ts].[SeenDate],
          [ts].[ServeDate],
          [ts].[AnnulDate]
      FROM TicketStatuses [ts]
      WHERE [ts].[MessageId] = [t].[MessageId]
      ORDER BY [ts].[TicketStatusId] DESC
    ) AS e
    WHERE mr.DateReceived >= @from
      AND mr.DateReceived < @to
      AND e.[Status] = 1

  UNION

    SELECT
      2 AS [Status],
      COUNT(t.MessageId)
    FROM Tickets t
    INNER JOIN MessageRecipients mr ON t.MessageId = mr.MessageId
    CROSS APPLY (
      SELECT TOP 1
          [ts].[Status],
          [ts].[SeenDate],
          [ts].[ServeDate],
          [ts].[AnnulDate]
      FROM TicketStatuses [ts]
      WHERE [ts].[MessageId] = [t].[MessageId]
      ORDER BY [ts].[TicketStatusId] DESC
    ) AS e
    WHERE e.ServeDate >= @from
      AND e.ServeDate < @to
      AND e.[Status] = 2

  UNION

    SELECT
      99 AS [Status],
      COUNT(t.MessageId)
    FROM Tickets t
    INNER JOIN MessageRecipients mr ON t.MessageId = mr.MessageId
    CROSS APPLY (
      SELECT TOP 1
          [ts].[Status],
          [ts].[SeenDate],
          [ts].[ServeDate],
          [ts].[AnnulDate]
      FROM TicketStatuses [ts]
      WHERE [ts].[MessageId] = [t].[MessageId]
      ORDER BY [ts].[TicketStatusId] DESC
    ) AS e
    WHERE e.AnnulDate >= @from
      AND e.AnnulDate < @to
      AND e.[Status] = 99

  OPEN status_cursor;
  FETCH NEXT FROM status_cursor INTO @_status, @internalServed;

  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM status_cursor INTO @_status, @externalServed;

  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM status_cursor INTO @_status, @annulled;

  CLOSE status_cursor;
  DEALLOCATE status_cursor;

  --нотификации
  DECLARE
    @_method NVARCHAR(10),
    @smsNotifications INT,
    @viberNotifications INT;

  DECLARE notification_cursor CURSOR FOR
    SELECT
      'Email' AS DeliveryMethod,
      COUNT(*) AS [Count]
    FROM [reports].[EmailDelivery]
    WHERE [Tag] = 'Tickets'
      AND [Status] = 0
      AND SentDate >= @from
      AND SentDate < @to

  UNION ALL

    SELECT
      'Sms' AS DeliveryMethod,
      COUNT(*) AS [Count]
    FROM [reports].[SmsDelivery]
    WHERE [Tag] = 'Tickets'
      AND [Status] = 0
      AND SentDate >= @from
      AND SentDate < @to

  UNION ALL

    SELECT
      'Viber' AS DeliveryMethod,
      COUNT(*) AS [Count]
    FROM [reports].[ViberDelivery]
    WHERE [Tag] = 'Tickets'
      AND [Status] = 0
      AND SentDate >= @from
      AND SentDate < @to

  OPEN notification_cursor;
  FETCH NEXT FROM notification_cursor INTO @_method, @emailNotifications; -- !

  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM notification_cursor INTO @_method, @smsNotifications;

  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM notification_cursor INTO @_method, @viberNotifications;

  CLOSE notification_cursor;
  DEALLOCATE notification_cursor;

  SET @phoneNotifications = @smsNotifications + @viberNotifications; -- !

  --връчени през ССЕВ
DECLARE
    @deliveredTicketPublicAdministrations INT,
    @deliveredPenalDecreePublicAdministrations INT,
    @deliveredTicketSocialOrganizations INT,
    @deliveredPenalDecreeSocialOrganizations INT;

  DECLARE delivered_cursor CURSOR FOR
    SELECT
      all_combinations.Type,
      all_combinations.TargetGroupId,
      COALESCE(aggregated_counts.MessageCount, 0) AS MessageCount
    FROM
      (SELECT DISTINCT t.Type, tgp.TargetGroupId
      FROM Tickets t
      CROSS JOIN TargetGroupProfiles tgp
      WHERE tgp.TargetGroupId in (1,2,3,4)) AS all_combinations
      LEFT JOIN
        (SELECT
            t.Type,
            tgp.TargetGroupId,
            COUNT(m.MessageId) AS MessageCount
        FROM Tickets t
            JOIN Messages m ON t.MessageId = m.MessageId
            JOIN MessageRecipients mr ON m.MessageId = mr.MessageId
            JOIN Profiles rp ON mr.ProfileId = rp.Id
            JOIN TargetGroupProfiles tgp ON rp.Id = tgp.ProfileId
        WHERE mr.DateReceived >= @from AND mr.DateReceived < @to
        GROUP BY t.Type, tgp.TargetGroupId) AS aggregated_counts ON all_combinations.Type = aggregated_counts.Type AND all_combinations.TargetGroupId = aggregated_counts.TargetGroupId
    ORDER BY all_combinations.TargetGroupId, all_combinations.Type;

  -- individuals
  OPEN delivered_cursor;
  FETCH NEXT FROM delivered_cursor INTO @_type, @_targetGroupId, @deliveredTicketIndividuals; -- !

  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM delivered_cursor INTO @_type, @_targetGroupId, @deliveredPenalDecreeIndividuals; -- !

  -- legal entities
  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM delivered_cursor INTO @_type, @_targetGroupId, @deliveredTicketLegalEntites; -- !

  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM delivered_cursor INTO @_type, @_targetGroupId, @deliveredPenalDecreeLegalEntites; -- !

  -- public administrations
  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM delivered_cursor INTO @_type, @_targetGroupId, @deliveredTicketPublicAdministrations;

  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM delivered_cursor INTO @_type, @_targetGroupId, @deliveredPenalDecreePublicAdministrations;

  -- social organizations
  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM delivered_cursor INTO @_type, @_targetGroupId, @deliveredTicketSocialOrganizations;

  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM delivered_cursor INTO @_type, @_targetGroupId, @deliveredPenalDecreeSocialOrganizations;

  CLOSE delivered_cursor;
  DEALLOCATE delivered_cursor;

  SET @deliveredTicketLegalEntites = @deliveredTicketLegalEntites + @deliveredTicketPublicAdministrations + @deliveredTicketSocialOrganizations; -- !
  SET @deliveredPenalDecreeLegalEntites = @deliveredPenalDecreeLegalEntites + @deliveredPenalDecreePublicAdministrations + @deliveredPenalDecreeSocialOrganizations; -- !

  --връчени през ССЕВ активни/ пасивни
  DECLARE @_passive INT;

  DECLARE active_passive_cursor CURSOR FOR
    SELECT
      all_combinations.IsPassive,
      COALESCE(aggregated_counts.MessageCount, 0) AS MessageCount
    FROM
      (SELECT DISTINCT rp.IsPassive FROM Profiles rp) AS all_combinations
      LEFT JOIN
        (SELECT
          rp.IsPassive,
          COUNT(m.MessageId) AS MessageCount
        FROM Tickets t
        JOIN Messages m ON t.MessageId = m.MessageId
        JOIN MessageRecipients mr ON m.MessageId = mr.MessageId
        JOIN Profiles rp ON mr.ProfileId = rp.Id
        WHERE m.DateSent >= @from AND m.DateSent < @to
        GROUP BY rp.IsPassive) AS aggregated_counts ON all_combinations.IsPassive = aggregated_counts.IsPassive
    ORDER BY all_combinations.IsPassive;

  OPEN active_passive_cursor;
  FETCH NEXT FROM active_passive_cursor INTO @_passive, @sentToActiveProfiles;

  IF @@FETCH_STATUS = 0
    FETCH NEXT FROM active_passive_cursor INTO @_passive, @sentToPassiveProfiles;

  CLOSE active_passive_cursor;
  DEALLOCATE active_passive_cursor;

  RETURN;

GO
