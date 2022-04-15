SET QUOTED_IDENTIFIER ON
GO

SET NOCOUNT ON
GO

PRINT '------ Creating Database'
:setvar rootPath ./
:r $(rootPath)/Create/CreateDB.sql
:r $(rootPath)/Create/Create.sql

PRINT '------ Done'
