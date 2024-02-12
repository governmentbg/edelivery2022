USE msdb;
GO

EXEC dbo.sp_add_job
  @job_name = N'Email_delivery_partitions_check',
  @owner_login_name = 'sa';
GO  

EXEC sp_add_jobstep
  @job_name = N'Email_delivery_partitions_check',
  @step_name = N'Date_check',
  @subsystem = N'TSQL',
  @command = N'IF EXISTS (SELECT * FROM [ElectronicDelivery].[reports].[EmailDelivery] WHERE [SentDate] > ''12-15-2025'' ) THROW 51000, ''A record exists with a date greater than the one specified!'', 1;',
  @retry_attempts = 0,
  @retry_interval = 0;
GO

EXEC dbo.sp_add_schedule
  @schedule_name = N'Email_delivery_partitions_check_schedule',
  @freq_type = 4,
  @freq_interval = 1,
  @freq_subday_type = 1,
  @active_start_date = 20231108,
  @active_start_time = 233000 ;
GO

EXEC sp_attach_schedule
  @job_name = N'Email_delivery_partitions_check',
  @schedule_name = N'Email_delivery_partitions_check_schedule';
GO

EXEC dbo.sp_add_jobserver
  @job_name = N'Email_delivery_partitions_check';
GO
