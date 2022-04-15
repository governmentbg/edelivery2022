GO
PRINT 'Addresses'
GO

CREATE TABLE [dbo].[Addresses](
  [AddressId] [int] IDENTITY(1,1) NOT NULL,
  [Country] [nchar](2) NULL,
  [State] [nvarchar](100) NULL,
  [City] [nvarchar](100) NULL,
  [Residence] [nvarchar](512) NULL,

  CONSTRAINT [PK_Addresses] PRIMARY KEY ([AddressId]),
  CONSTRAINT [FK_Addresses_Countries]
    FOREIGN KEY([Country])
    REFERENCES [dbo].[Countries] ([CountryISO2]),
)
GO
