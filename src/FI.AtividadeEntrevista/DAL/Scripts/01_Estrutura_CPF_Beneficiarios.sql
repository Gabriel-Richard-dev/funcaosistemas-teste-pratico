/*
    Estrutura de banco para:
      - Campo CPF do cliente (CLIENTES.CPF)
      - Cadastro de beneficiarios (BENEFICIARIOS)

    O CPF e armazenado apenas com os digitos (11 caracteres), sem mascara.
    Script idempotente: pode ser executado mais de uma vez sem erro.
    Apos executa-lo, executar os scripts de procedures em:
      - DAL\Clientes\Procedures
      - DAL\Beneficiarios\Procedures
*/

/* ---------- CLIENTES.CPF ---------- */
IF COL_LENGTH('dbo.CLIENTES', 'CPF') IS NULL
    ALTER TABLE dbo.CLIENTES ADD CPF VARCHAR(11) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_CLIENTES_CPF' AND object_id = OBJECT_ID('dbo.CLIENTES'))
    ALTER TABLE dbo.CLIENTES ADD CONSTRAINT UQ_CLIENTES_CPF UNIQUE (CPF);
GO

/* ---------- BENEFICIARIOS ---------- */
IF OBJECT_ID('dbo.BENEFICIARIOS', 'U') IS NULL
    CREATE TABLE dbo.BENEFICIARIOS
    (
        ID        BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CPF       VARCHAR(11) NOT NULL,
        NOME      VARCHAR(50) NOT NULL,
        IDCLIENTE BIGINT      NOT NULL
    );
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BENEFICIARIOS_CLIENTES')
    ALTER TABLE dbo.BENEFICIARIOS
        ADD CONSTRAINT FK_BENEFICIARIOS_CLIENTES FOREIGN KEY (IDCLIENTE) REFERENCES dbo.CLIENTES (ID);
GO

/* Nao permite o mesmo CPF de beneficiario mais de uma vez para o mesmo cliente */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_BENEFICIARIOS_CLIENTE_CPF' AND object_id = OBJECT_ID('dbo.BENEFICIARIOS'))
    ALTER TABLE dbo.BENEFICIARIOS ADD CONSTRAINT UQ_BENEFICIARIOS_CLIENTE_CPF UNIQUE (IDCLIENTE, CPF);
GO
