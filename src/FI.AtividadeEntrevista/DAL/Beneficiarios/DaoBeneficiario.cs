using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace FI.AtividadeEntrevista.DAL
{
    /// <summary>
    /// Classe de acesso a dados de Beneficiario
    /// </summary>
    internal class DaoBeneficiario : AcessoDados
    {
        internal DaoBeneficiario(Transacao transacao = null) : base(transacao)
        {
        }

        /// <summary>
        /// Inclui um novo beneficiario
        /// </summary>
        /// <param name="beneficiario">Objeto de beneficiario</param>
        /// <returns>Id gerado</returns>
        internal long Incluir(DML.Beneficiario beneficiario)
        {
            List<SqlParameter> parametros = new List<SqlParameter>();

            parametros.Add(new SqlParameter("NOME", beneficiario.Nome));
            parametros.Add(new SqlParameter("CPF", beneficiario.CPF));
            parametros.Add(new SqlParameter("IDCLIENTE", beneficiario.IdCliente));

            DataSet ds = base.Consultar("FI_SP_IncBenef", parametros);
            long ret = 0;
            if (ds.Tables[0].Rows.Count > 0)
                long.TryParse(ds.Tables[0].Rows[0][0].ToString(), out ret);
            return ret;
        }

        /// <summary>
        /// Altera um beneficiario do cliente
        /// </summary>
        /// <param name="beneficiario">Objeto de beneficiario</param>
        internal void Alterar(DML.Beneficiario beneficiario)
        {
            List<SqlParameter> parametros = new List<SqlParameter>();

            parametros.Add(new SqlParameter("NOME", beneficiario.Nome));
            parametros.Add(new SqlParameter("CPF", beneficiario.CPF));
            parametros.Add(new SqlParameter("ID", beneficiario.Id));
            parametros.Add(new SqlParameter("IDCLIENTE", beneficiario.IdCliente));

            base.Executar("FI_SP_AltBenef", parametros);
        }

        /// <summary>
        /// Exclui um beneficiario do cliente
        /// </summary>
        /// <param name="id">Id do beneficiario</param>
        /// <param name="idCliente">Id do cliente</param>
        internal void Excluir(long id, long idCliente)
        {
            List<SqlParameter> parametros = new List<SqlParameter>();

            parametros.Add(new SqlParameter("ID", id));
            parametros.Add(new SqlParameter("IDCLIENTE", idCliente));

            base.Executar("FI_SP_DelBenef", parametros);
        }

        /// <summary>
        /// Lista os beneficiarios de um cliente
        /// </summary>
        /// <param name="idCliente">Id do cliente</param>
        internal List<DML.Beneficiario> Listar(long idCliente)
        {
            List<SqlParameter> parametros = new List<SqlParameter>();

            parametros.Add(new SqlParameter("IDCLIENTE", idCliente));

            DataSet ds = base.Consultar("FI_SP_ConsBenef", parametros);
            return Converter(ds);
        }

        private List<DML.Beneficiario> Converter(DataSet ds)
        {
            List<DML.Beneficiario> lista = new List<DML.Beneficiario>();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    lista.Add(new DML.Beneficiario()
                    {
                        Id = row.Field<long>("ID"),
                        CPF = row.Field<string>("CPF"),
                        Nome = row.Field<string>("NOME"),
                        IdCliente = row.Field<long>("IDCLIENTE")
                    });
                }
            }

            return lista;
        }
    }
}
