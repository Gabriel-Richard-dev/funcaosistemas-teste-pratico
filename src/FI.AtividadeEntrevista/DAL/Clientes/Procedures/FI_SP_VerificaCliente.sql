/*
	Verifica se ja existe cliente com o CPF informado.
	@ID: id do cliente que deve ser desconsiderado (alteracao). Na inclusao, informar 0.
*/
CREATE OR ALTER PROC FI_SP_VerificaCliente
	@CPF VARCHAR(11),
	@ID  BIGINT = 0
AS
BEGIN
	SELECT 1 FROM CLIENTES WITH(NOLOCK) WHERE CPF = @CPF AND ID <> ISNULL(@ID, 0)
END
