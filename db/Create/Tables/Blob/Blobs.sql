GO
PRINT 'Blobs'
GO

CREATE SEQUENCE [dbo].[BlobsIdSequence]
AS
    INT START WITH 1000
    INCREMENT BY 1
    NO CYCLE;
GO

CREATE TABLE [dbo].[Blobs](
  [BlobId] [int] NOT NULL,
  [ROWGUID] [uniqueidentifier] ROWGUIDCOL  NOT NULL
    CONSTRAINT [DEFAULT_ROWGUID] DEFAULT NEWSEQUENTIALID(),
  [FileName] [nvarchar](500) NOT NULL,
  [EncryptedContent] [varbinary](max) FILESTREAM  NOT NULL,
  [IV] [binary](16) NOT NULL,
  [Hash] [nvarchar](64) NULL,
  [HashAlgorithm] [nvarchar](10) NULL,
  [Timestamp] [varbinary](8000) NULL,
  [Size] [bigint] NULL,
  [MalwareScanResultId] [int] NULL,
  [DocumentRegistrationNumber] [nvarchar](255) NULL,
  [CreateDate] [datetime2](7) NOT NULL,
  [ModifyDate] [datetime2](7) NOT NULL,
  [Version] [timestamp] NOT NULL,
  [FileExtension] AS IIF([FileName] IS NULL, NULL, Right([FileName], CHARINDEX('.', REVERSE([FileName])))) PERSISTED
  
  CONSTRAINT [PK_Blobs] PRIMARY KEY ([BlobId]),
  CONSTRAINT [UQ_Blobs] UNIQUE ([ROWGUID]) ON [PRIMARY],
  CONSTRAINT [FK_Blobs_MalwareScanResult]
    FOREIGN KEY([MalwareScanResultId])
    REFERENCES [dbo].[MalwareScanResult] ([Id])
)
ON [psBlobs]([BlobId])
FILESTREAM_ON [psBlobsFileStream]
GO
