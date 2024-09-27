USE msdb;
GO

BEGIN TRANSACTION

DECLARE @ReturnCode INT
SELECT @ReturnCode = 0

DECLARE @jobId BINARY(16)

EXEC @ReturnCode = msdb.dbo.sp_add_job
  @job_name=N'TicketStats - ElectronicDelivery - REPORT',
  @enabled=0,
  @notify_level_eventlog=2,
  @notify_level_email=2,
  @notify_level_netsend=0,
  @notify_level_page=0,
  @delete_level=0,
  @description=N'N/A',
  @category_name=N'Data Collector',
  @owner_login_name=N'sa',
  @notify_email_operator_name=N'Ciela EDelivery Team',
  @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

EXEC @ReturnCode = msdb.dbo.sp_add_jobstep
  @job_id=@jobId,
  @step_name=N'Generate data and insert it',
  @step_id=1,
  @cmdexec_success_code=0,
  @on_success_step_id=0,
  @on_fail_action=2,
  @on_fail_step_id=0,
  @os_run_priority=0,
  @subsystem=N'TSQL',
  @database_name = N'ElectronicDelivery',
  @retry_attempts = 0,
  @retry_interval = 0,
  @on_success_action = 3,
  @command =
N'USE [ElectronicDelivery]
  GO

  DECLARE @currentDate DATE = DATEADD(day, -1, GETDATE());

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

  EXEC [dbo].[spGetTicketStats]
    @currentDate,
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
      @currentDate,
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
';
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

EXEC @ReturnCode = msdb.dbo.sp_add_jobstep
  @job_id=@jobId,
  @step_name=N'Send email',
  @step_id=2,
  @cmdexec_success_code=0,
  @on_success_step_id=0,
  @on_fail_action=2,
  @on_fail_step_id=0,
  @os_run_priority=0,
  @subsystem=N'TSQL',
  @database_name = N'ElectronicDelivery',
  @retry_attempts = 0,
  @retry_interval = 0,
  @on_success_action = 1,
  @command =
N'DECLARE @reportQuery NVARCHAR(max);
  SET @reportQuery =
    N''DECLARE @header NVARCHAR(MAX);
      DECLARE @currentDate DATE = DATEADD(day, -1, GETDATE());
      SET @header = N''''"Дата", "Брой получени в ССЕВ", "Фиш - ФЛ", "Фиш - ЮЛ", "НП - ФЛ", "НП - ЮЛ", "Връчен в ССЕВ", "Връчени външно", "Анулирани", "Нотификации Общо", "Имейл", "Телефон", "Брой връчени през ССЕВ", "Връчени Фиш - ФЛ", "Връчени Фиш - ЮЛ", "Връчени НП - ФЛ", "Връчени НП - ЮЛ", "Пасивни профили", "Активни профили"'''';

      SELECT @header + CHAR(13) + CHAR(10) +
        CONCAT_WS( '''','''',
          TicketStatDate,
          ReceivedTicketIndividuals + ReceivedPenalDecreeIndividuals + ReceivedTicketLegalEntites + ReceivedPenalDecreeLegalEntites,
          ReceivedTicketIndividuals,
          ReceivedTicketLegalEntites,
          ReceivedPenalDecreeIndividuals,
          ReceivedPenalDecreeLegalEntites,
          InternalServed,
          ExternalServed,
          Annulled,
          EmailNotifications + PhoneNotifications,
          EmailNotifications,
          PhoneNotifications,
          DeliveredTicketIndividuals + DeliveredPenalDecreeIndividuals + DeliveredTicketLegalEntites + DeliveredPenalDecreeLegalEntites,
          DeliveredTicketIndividuals,
          DeliveredTicketLegalEntites,
          DeliveredPenalDecreeIndividuals,
          DeliveredPenalDecreeLegalEntites,
          SentToPassiveProfiles,
          SentToActiveProfiles)
      FROM [ElectronicDelivery].[reports].[TicketStats]
      WHERE TicketStatDate = @currentDate'';

      EXEC msdb.dbo.sp_send_dbmail
          @profile_name = ''Test notifications'',
          @recipients = ''ygalyov@ciela.com;'',
          @subject = ''Tickets report'',
          @body_format = ''TEXT'',
          @body = '''',
          @execute_query_database = ''ElectronicDelivery'',
          @query = @reportQuery,
          @attach_query_result_as_file = 1,
          @query_attachment_filename = ''report.csv'',
          @query_result_header = 0,
          @query_result_width = 1024;';
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule
  @job_id=@jobId,
  @name=N'Every Morning at 04:00:00',
  @enabled=0,
  @freq_type=4,
  @freq_interval=1,
  @freq_subday_type=1,
  @active_start_date=20231108,
  @active_start_time=040000
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO
