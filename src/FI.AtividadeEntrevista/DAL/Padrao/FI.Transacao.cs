using System;
using System.Configuration;
using System.Data.SqlClient;

namespace FI.AtividadeEntrevista.DAL
{
    /// <summary>
    /// Transacao de banco compartilhada entre os DAOs, para que varias operacoes
    /// (ex.: cliente + beneficiarios) sejam gravadas de forma atomica.
    /// Se Confirmar() nao for chamado, as alteracoes sao desfeitas no Dispose().
    /// </summary>
    internal class Transacao : IDisposable
    {
        internal SqlConnection Conexao { get; private set; }
        internal SqlTransaction TransacaoSql { get; private set; }

        private bool confirmada;

        internal Transacao()
        {
            ConnectionStringSettings conn = ConfigurationManager.ConnectionStrings["BancoDeDados"];
            Conexao = new SqlConnection(conn != null ? conn.ConnectionString : string.Empty);
            Conexao.Open();
            TransacaoSql = Conexao.BeginTransaction();
        }

        internal void Confirmar()
        {
            TransacaoSql.Commit();
            confirmada = true;
        }

        public void Dispose()
        {
            try
            {
                if (!confirmada && TransacaoSql.Connection != null)
                    TransacaoSql.Rollback();
            }
            finally
            {
                TransacaoSql.Dispose();
                Conexao.Dispose();
            }
        }
    }
}
