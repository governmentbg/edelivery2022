GO
PRINT 'UpdateScripts'
GO

CREATE TABLE [dbo].[UpdateScripts](
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [ScriptName] [nvarchar](50) NOT NULL,
  [Applied] [datetime2](7) NOT NULL,
  
  UNIQUE ([ScriptName]),
)
GO
