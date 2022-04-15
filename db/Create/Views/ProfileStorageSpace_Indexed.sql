GO
PRINT 'ProfileStorageSpace_Indexed'
GO

CREATE VIEW [dbo].[ProfileStorageSpace_Indexed]
WITH SCHEMABINDING AS
  SELECT p.[Id] as ProfileId, SUM(ISNULL(b.[Size], 0)) as UsedStorageSpace, COUNT_BIG(*) as BlobsCount
  FROM [dbo].[Profiles] p
  JOIN [dbo].[ProfileBlobAccessKeys] pbak on p.[Id] = pbak.[ProfileId]
  JOIN [dbo].[Blobs] b on pbak.[BlobId] = b.[BlobId]
  WHERE p.[Id] <> 1 -- SystemProfileId
  AND pbak.[Type] = 1 /*Storage*/
  GROUP BY p.[Id]
GO

CREATE UNIQUE CLUSTERED INDEX [IX_ProfileStorageSpace_Indexed]
  ON [dbo].[ProfileStorageSpace_Indexed] (ProfileId);
GO
