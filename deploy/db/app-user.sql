/*
    Cria (ou atualiza a senha) do login usado pela aplicacao.
    Permissoes: leitura, escrita e execucao de procedures no banco da aplicacao (sem DDL).
    Variaveis sqlcmd: $(AppUser), $(AppPassword)
*/
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'$(AppUser)')
    CREATE LOGIN [$(AppUser)] WITH PASSWORD = N'$(AppPassword)', CHECK_POLICY = ON;
ELSE
    ALTER LOGIN [$(AppUser)] WITH PASSWORD = N'$(AppPassword)';
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$(AppUser)')
    CREATE USER [$(AppUser)] FOR LOGIN [$(AppUser)];
GO

ALTER ROLE db_datareader ADD MEMBER [$(AppUser)];
ALTER ROLE db_datawriter ADD MEMBER [$(AppUser)];
GRANT EXECUTE TO [$(AppUser)];
GO
