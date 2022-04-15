GO
PRINT 'Individuals'
GO

CREATE TABLE [dbo].[Individuals](
  [IndividualId] [uniqueidentifier] NOT NULL,
  [FirstName] [nvarchar](100) NOT NULL,
  [MiddleName] [nvarchar](100) NOT NULL,
  [LastName] [nvarchar](100) NOT NULL,

  CONSTRAINT [PK_Individuals] PRIMARY KEY ([IndividualId]),
  CONSTRAINT [FK_Individuals_Profiles]
    FOREIGN KEY([IndividualId])
    REFERENCES [dbo].[Profiles] ([ElectronicSubjectId]),
)
GO
