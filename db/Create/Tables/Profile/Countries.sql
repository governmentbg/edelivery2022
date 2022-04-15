GO
PRINT 'Countries'
GO

CREATE TABLE [dbo].[Countries](
  [CountryISO2] [nchar](2) NOT NULL,
  [Name] [nvarchar](50) NOT NULL,
  
  CONSTRAINT [PK_Countries] PRIMARY KEY ([CountryISO2]),
)
GO
