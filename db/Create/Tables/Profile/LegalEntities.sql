GO
PRINT 'LegalEntities'
GO

CREATE TABLE [dbo].[LegalEntities](
  [LegalEntityId] [uniqueidentifier] NOT NULL,
  [Name] [nvarchar](512) NOT NULL,
  [ParentLegalEntityId] [uniqueidentifier] NULL,

  CONSTRAINT [PK_LegalEntities] PRIMARY KEY ([LegalEntityId]),
  CONSTRAINT [FK_LegalEntities_LegalEntities]
    FOREIGN KEY([ParentLegalEntityId])
    REFERENCES [dbo].[LegalEntities] ([LegalEntityId]),
  CONSTRAINT [FK_LegalEntities_Profiles]
    FOREIGN KEY([LegalEntityId])
    REFERENCES [dbo].[Profiles] ([ElectronicSubjectId]),
)
GO
