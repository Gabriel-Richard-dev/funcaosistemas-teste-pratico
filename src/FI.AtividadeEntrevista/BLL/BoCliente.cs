using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FI.AtividadeEntrevista.Util;

namespace FI.AtividadeEntrevista.BLL
{
    public class BoCliente
    {
        /// <summary>
        /// Codigos de erro do SQL Server para violacao de chave unica
        /// </summary>
        private static readonly int[] ErrosChaveDuplicada = { 2601, 2627 };

        /// <summary>
        /// Inclui um novo cliente juntamente com seus beneficiarios
        /// </summary>
        /// <param name="cliente">Objeto de cliente</param>
        public long Incluir(DML.Cliente cliente)
        {
            PrepararEValidar(cliente);

            try
            {
                using (DAL.Transacao transacao = new DAL.Transacao())
                {
                    DAL.DaoCliente cli = new DAL.DaoCliente(transacao);

                    if (cli.VerificarExistencia(cliente.CPF))
                        throw new RegraNegocioException(MensagemCpfClienteDuplicado(cliente.CPF));

                    cliente.Id = cli.Incluir(cliente);

                    DAL.DaoBeneficiario ben = new DAL.DaoBeneficiario(transacao);
                    foreach (DML.Beneficiario beneficiario in cliente.Beneficiarios)
                    {
                        beneficiario.IdCliente = cliente.Id;
                        beneficiario.Id = ben.Incluir(beneficiario);
                    }

                    transacao.Confirmar();
                }
            }
            catch (SqlException ex) when (ErrosChaveDuplicada.Contains(ex.Number))
            {
                throw new RegraNegocioException(MensagemChaveDuplicada(ex, cliente));
            }

            return cliente.Id;
        }

        /// <summary>
        /// Altera um cliente e sincroniza seus beneficiarios
        /// (inclui os novos, altera os existentes e exclui os removidos)
        /// </summary>
        /// <param name="cliente">Objeto de cliente</param>
        public void Alterar(DML.Cliente cliente)
        {
            PrepararEValidar(cliente);

            try
            {
                using (DAL.Transacao transacao = new DAL.Transacao())
                {
                    DAL.DaoCliente cli = new DAL.DaoCliente(transacao);

                    if (cli.VerificarExistencia(cliente.CPF, cliente.Id))
                        throw new RegraNegocioException(MensagemCpfClienteDuplicado(cliente.CPF));

                    cli.Alterar(cliente);

                    SincronizarBeneficiarios(cliente, transacao);

                    transacao.Confirmar();
                }
            }
            catch (SqlException ex) when (ErrosChaveDuplicada.Contains(ex.Number))
            {
                throw new RegraNegocioException(MensagemChaveDuplicada(ex, cliente));
            }
        }

        /// <summary>
        /// Consulta o cliente pelo id, juntamente com seus beneficiarios
        /// </summary>
        /// <param name="id">id do cliente</param>
        /// <returns></returns>
        public DML.Cliente Consultar(long id)
        {
            DAL.DaoCliente cli = new DAL.DaoCliente();
            DML.Cliente cliente = cli.Consultar(id);

            if (cliente != null)
                cliente.Beneficiarios = new DAL.DaoBeneficiario().Listar(cliente.Id);

            return cliente;
        }

        /// <summary>
        /// Excluir o cliente pelo id
        /// </summary>
        /// <param name="id">id do cliente</param>
        /// <returns></returns>
        public void Excluir(long id)
        {
            DAL.DaoCliente cli = new DAL.DaoCliente();
            cli.Excluir(id);
        }

        /// <summary>
        /// Lista os clientes
        /// </summary>
        public List<DML.Cliente> Listar()
        {
            DAL.DaoCliente cli = new DAL.DaoCliente();
            return cli.Listar();
        }

        /// <summary>
        /// Lista os clientes
        /// </summary>
        public List<DML.Cliente> Pesquisa(int iniciarEm, int quantidade, string campoOrdenacao, bool crescente, out int qtd)
        {
            DAL.DaoCliente cli = new DAL.DaoCliente();
            return cli.Pesquisa(iniciarEm,  quantidade, campoOrdenacao, crescente, out qtd);
        }

        /// <summary>
        /// VerificaExistencia
        /// </summary>
        /// <param name="CPF">CPF, com ou sem mascara</param>
        /// <param name="idIgnorar">Id do cliente a ser desconsiderado (alteracao)</param>
        /// <returns></returns>
        public bool VerificarExistencia(string CPF, long idIgnorar = 0)
        {
            DAL.DaoCliente cli = new DAL.DaoCliente();
            return cli.VerificarExistencia(Cpf.Limpar(CPF), idIgnorar);
        }

        /// <summary>
        /// Normaliza os CPFs (somente digitos) e valida as regras de cliente e beneficiarios
        /// antes de qualquer gravacao
        /// </summary>
        private static void PrepararEValidar(DML.Cliente cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            if (!Cpf.Validar(cliente.CPF))
                throw new RegraNegocioException("CPF do cliente inválido.");

            cliente.CPF = Cpf.Limpar(cliente.CPF);

            if (cliente.Beneficiarios == null)
                cliente.Beneficiarios = new List<DML.Beneficiario>();

            HashSet<string> cpfsBeneficiarios = new HashSet<string>();
            foreach (DML.Beneficiario beneficiario in cliente.Beneficiarios)
            {
                beneficiario.Nome = (beneficiario.Nome ?? string.Empty).Trim();

                if (beneficiario.Nome.Length == 0)
                    throw new RegraNegocioException("Informe o nome do beneficiário.");

                if (beneficiario.Nome.Length > 50)
                    throw new RegraNegocioException(string.Format("O nome do beneficiário \"{0}\" deve possuir no máximo 50 caracteres.", beneficiario.Nome));

                if (!Cpf.Validar(beneficiario.CPF))
                    throw new RegraNegocioException(string.Format("CPF do beneficiário \"{0}\" inválido.", beneficiario.Nome));

                beneficiario.CPF = Cpf.Limpar(beneficiario.CPF);

                if (!cpfsBeneficiarios.Add(beneficiario.CPF))
                    throw new RegraNegocioException(string.Format("O CPF {0} foi informado para mais de um beneficiário.", Cpf.Formatar(beneficiario.CPF)));
            }
        }

        /// <summary>
        /// Exclui primeiro os beneficiarios removidos, depois altera os existentes e por fim inclui os novos
        /// </summary>
        private static void SincronizarBeneficiarios(DML.Cliente cliente, DAL.Transacao transacao)
        {
            DAL.DaoBeneficiario ben = new DAL.DaoBeneficiario(transacao);

            HashSet<long> idsAtuais = new HashSet<long>(ben.Listar(cliente.Id).Select(b => b.Id));
            HashSet<long> idsMantidos = new HashSet<long>(cliente.Beneficiarios.Where(b => idsAtuais.Contains(b.Id)).Select(b => b.Id));

            foreach (long id in idsAtuais.Where(id => !idsMantidos.Contains(id)))
                ben.Excluir(id, cliente.Id);

            foreach (DML.Beneficiario beneficiario in cliente.Beneficiarios)
            {
                beneficiario.IdCliente = cliente.Id;

                if (idsMantidos.Contains(beneficiario.Id))
                    ben.Alterar(beneficiario);
            }

            foreach (DML.Beneficiario beneficiario in cliente.Beneficiarios.Where(b => !idsMantidos.Contains(b.Id)))
                beneficiario.Id = ben.Incluir(beneficiario);
        }

        private static string MensagemCpfClienteDuplicado(string cpf)
        {
            return string.Format("Já existe um cliente cadastrado com o CPF {0}.", Cpf.Formatar(cpf));
        }

        /// <summary>
        /// Traduz a violacao de chave unica do banco (concorrencia) para uma mensagem amigavel
        /// </summary>
        private static string MensagemChaveDuplicada(SqlException ex, DML.Cliente cliente)
        {
            if (ex.Message.Contains("UQ_BENEFICIARIOS_CLIENTE_CPF"))
                return "Não é permitido mais de um beneficiário com o mesmo CPF para o mesmo cliente.";

            return MensagemCpfClienteDuplicado(cliente.CPF);
        }
    }
}
