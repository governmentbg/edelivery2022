GO
PRINT 'BlobSignatures'
GO

CREATE SEQUENCE [dbo].[BlobSignaturesIdSequence]
AS
    INT START WITH 1000
    INCREMENT BY 1
    NO CYCLE;
GO

CREATE TABLE [dbo].[BlobSignatures](
  [BlobSignatureId] [int] NOT NULL,
  [BlobId] [int] NOT NULL,
  [X509Certificate2DER] [varbinary](max) NULL,
  [CoversDocument] [bit] NOT NULL,
  [CoversPriorRevision] [bit] NOT NULL,
  [SignDate] [datetime2](7) NOT NULL,
  [IsTimestamp] [bit] NOT NULL,
  [ValidAtTimeOfSigning] [bit] NOT NULL,
  [Issuer] [nvarchar](1000) NOT NULL,
  [Subject] [nvarchar](1000) NOT NULL,
  [SerialNumber] [nvarchar](100) NOT NULL,
  [Version] [int] NOT NULL,
  [ValidFrom] [datetime2](7) NOT NULL,
  [ValidTo] [datetime2](7) NOT NULL,
  
  CONSTRAINT [PK_BlobSignatures] PRIMARY KEY NONCLUSTERED ([BlobSignatureId]),
  CONSTRAINT [UK_BlobSignatures] UNIQUE CLUSTERED ([BlobId], [BlobSignatureId]),
  CONSTRAINT [FK_BlobSignatures_Blobs]
    FOREIGN KEY([BlobId])
    REFERENCES [dbo].[Blobs] ([BlobId]),
)
GO
