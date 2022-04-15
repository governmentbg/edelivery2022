GO
PRINT 'Roles'
GO

CREATE TABLE [dbo].[Roles](
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [Name] [nvarchar](50) NOT NULL,
  
  CONSTRAINT [PK_Roles] PRIMARY KEY ([Id]),
)
GO
