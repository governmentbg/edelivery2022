USE [master]
GO

print 'Create database $(dbName)'
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'$(dbName)')
BEGIN
    ALTER DATABASE [$(dbName)] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
    DROP DATABASE [$(dbName)]
END
GO

DECLARE @SQL NVARCHAR(MAX) = ''

SELECT @SQL = @SQL + '
    CREATE DATABASE [$(dbName)] ON
    PRIMARY
    (
        NAME = [$(dbName)],
        FILENAME = ''' + CAST(SERVERPROPERTY('INSTANCEDEFAULTDATAPATH') AS NVARCHAR(MAX)) + '$(dbName).mdf''
    ),
    FILEGROUP [FG_Blobs_1] CONTAINS FILESTREAM
    (
        NAME = [$(dbName)_Blobs_1_01],
        FILENAME = ''' + CAST(SERVERPROPERTY('INSTANCEDEFAULTDATAPATH') AS NVARCHAR(MAX)) + '$(dbName)_Blobs_1_01''
    ),
    (
        NAME = [$(dbName)_Blobs_1_02],
        FILENAME = ''' + CAST(SERVERPROPERTY('INSTANCEDEFAULTDATAPATH') AS NVARCHAR(MAX)) + '$(dbName)_Blobs_1_02''
    ),
    (
        NAME = [$(dbName)_Blobs_1_03],
        FILENAME = ''' + CAST(SERVERPROPERTY('INSTANCEDEFAULTDATAPATH') AS NVARCHAR(MAX)) + '$(dbName)_Blobs_1_03''
    ),
    (
        NAME = [$(dbName)_Blobs_1_04],
        FILENAME = ''' + CAST(SERVERPROPERTY('INSTANCEDEFAULTDATAPATH') AS NVARCHAR(MAX)) + '$(dbName)_Blobs_1_04''
    ),
    (
        NAME = [$(dbName)_Blobs_1_05],
        FILENAME = ''' + CAST(SERVERPROPERTY('INSTANCEDEFAULTDATAPATH') AS NVARCHAR(MAX)) + '$(dbName)_Blobs_1_05''
    ),
    (
        NAME = [$(dbName)_Blobs_1_06],
        FILENAME = ''' + CAST(SERVERPROPERTY('INSTANCEDEFAULTDATAPATH') AS NVARCHAR(MAX)) + '$(dbName)_Blobs_1_06''
    ),
    (
        NAME = [$(dbName)_Blobs_1_07],
        FILENAME = ''' + CAST(SERVERPROPERTY('INSTANCEDEFAULTDATAPATH') AS NVARCHAR(MAX)) + '$(dbName)_Blobs_1_07''
    ),
    (
        NAME = [$(dbName)_Blobs_1_08],
        FILENAME = ''' + CAST(SERVERPROPERTY('INSTANCEDEFAULTDATAPATH') AS NVARCHAR(MAX)) + '$(dbName)_Blobs_1_08''
    ),
    (
        NAME = [$(dbName)_Blobs_1_09],
        FILENAME = ''' + CAST(SERVERPROPERTY('INSTANCEDEFAULTDATAPATH') AS NVARCHAR(MAX)) + '$(dbName)_Blobs_1_09''
    ),
    (
        NAME = [$(dbName)_Blobs_1_10],
        FILENAME = ''' + CAST(SERVERPROPERTY('INSTANCEDEFAULTDATAPATH') AS NVARCHAR(MAX)) + '$(dbName)_Blobs_1_10''
    )
    LOG ON
    (
        NAME = [$(dbName)_log],
        FILENAME = '''  + CAST(SERVERPROPERTY('INSTANCEDEFAULTLOGPATH') AS NVARCHAR(MAX)) + '$(dbName)_log.ldf''
    )
    COLLATE SQL_Latin1_General_CP1_CI_AS';

EXEC sp_executesql @SQL
GO

ALTER DATABASE [$(dbName)] SET ALLOW_SNAPSHOT_ISOLATION ON
GO

ALTER DATABASE [$(dbName)] SET READ_COMMITTED_SNAPSHOT ON
GO

-- TODO investigate
-- ALTER DATABASE [$(dbName)] SET QUERY_STORE = ON
-- GO

USE [$(dbName)]
GO

-- create blobs partition function
CREATE PARTITION FUNCTION pfBlobs (INT)
AS RANGE RIGHT FOR VALUES (
  1,
  5000001 -- reserved, must be empty
);
GO

-- create blobs partition scheme
CREATE PARTITION SCHEME psBlobs
AS PARTITION pfBlobs ALL TO ([PRIMARY]);
GO

CREATE PARTITION SCHEME psBlobsFileStream
AS PARTITION pfBlobs ALL TO ([FG_Blobs_1]);
GO
