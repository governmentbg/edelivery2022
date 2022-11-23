PRINT 'Create spDeleteOrphanedBlobs'
GO

CREATE OR ALTER PROCEDURE [dbo].[spDeleteOrphanedBlobs]
AS

SET NOCOUNT ON;

BEGIN TRANSACTION;

BEGIN TRY

  CREATE TABLE #UsedBlobs (
    [BlobId] INT NOT NULL,
    PRIMARY KEY([BlobId])
  )
  CREATE TABLE #UnusedBlobs (
    [BlobId] INT NOT NULL,
    [Size] BIGINT NULL,
    PRIMARY KEY([BlobId])
  )

  INSERT INTO #UsedBlobs ([BlobId])
  SELECT rr.[BlobId]
  FROM [dbo].[RegistrationRequests] rr
  UNION
    SELECT mb.[BlobId]
    FROM [dbo].[MessageBlobs] mb
  UNION
    SELECT mr.[MessagePdfBlobId]
    FROM [dbo].[MessageRecipients] mr
    WHERE mr.[MessagePdfBlobId] IS NOT NULL
  UNION
    SELECT m.[MessagePdfBlobId]
    FROM [dbo].[Messages] m
    WHERE m.[MessagePdfBlobId] IS NOT NULL
  UNION
    SELECT pbak.[BlobId]
    FROM [dbo].[ProfileBlobAccessKeys] pbak
    WHERE pbak.[Type] <> 0

  INSERT INTO #UnusedBlobs ([BlobId], [Size])
  SELECT [BlobId], [Size]
  FROM [dbo].[Blobs] b
  WHERE
    b.[CreateDate] < DATEADD(day, -1, GETDATE())
    AND NOT EXISTS (
      SELECT NULL
      FROM #UsedBlobs ub
      WHERE ub.[BlobId] = b.[BlobId]
    )
    AND $PARTITION.pfBlobs(b.[BlobId]) IN (
      SELECT
        dds.destination_id
      FROM
        sys.partition_schemes ps
        INNER JOIN sys.destination_data_spaces dds ON ps.data_space_id = dds.partition_scheme_id
        INNER JOIN sys.filegroups fg ON dds.data_space_id = fg.data_space_id
      WHERE
        ps.name = 'psBlobsFileStream' AND
        fg.is_read_only = 0
    )

  DELETE pbak
  FROM [dbo].[ProfileBlobAccessKeys] pbak
  WHERE pbak.[Type] = 0
  AND EXISTS (
    SELECT NULL
    FROM #UnusedBlobs ub
    WHERE ub.[BlobId] = pbak.[BlobId]
  )

  DELETE bs
  FROM [dbo].[BlobSignatures] bs
  WHERE EXISTS (
    SELECT NULL
    FROM #UnusedBlobs ub
    WHERE ub.[BlobId] = bs.[BlobId]
  )

  PRINT 'Deleted ' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + ' BlobSignatures'

  DELETE b
  FROM [dbo].[Blobs] b
  WHERE EXISTS (
    SELECT NULL
    FROM #UnusedBlobs ub
    WHERE ub.[BlobId] = b.[BlobId]
  )

  DECLARE @DeletedCount INT = @@ROWCOUNT;
  DECLARE @TotalMegabytesDeleted INT;
  SELECT @TotalMegabytesDeleted = ISNULL(SUM(Size),0) / 1024 / 1024
  FROM #UnusedBlobs

  PRINT 'Deleted ' + CAST(@DeletedCount AS NVARCHAR(20)) + ' Blobs with total size of ' + CAST(@TotalMegabytesDeleted AS NVARCHAR(20)) + 'MB'

  DROP TABLE #UsedBlobs;
  DROP TABLE #UnusedBlobs;

  COMMIT;
END TRY
BEGIN CATCH

    DECLARE @error int,
            @message varchar(4000);

    SELECT
        @error = ERROR_NUMBER(),
        @message = ERROR_MESSAGE();

    ROLLBACK;

    RAISERROR ('An error ocurred in spDeleteOrphanedBlobs: %d: %s', 16, 1, @error, @message);
END CATCH;
GO
