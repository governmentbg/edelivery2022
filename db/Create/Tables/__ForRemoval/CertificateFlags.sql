GO
PRINT 'CertificateFlags'
GO

CREATE TABLE [dbo].[CertificateFlags](
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [CertificateThumbprint] [nvarchar](50) NOT NULL,
  [CanSendOnBehalfOfInstitution] [bit] NOT NULL,
  
  CONSTRAINT [PK_CertificateFlags] PRIMARY KEY ([Id]),
)
GO
