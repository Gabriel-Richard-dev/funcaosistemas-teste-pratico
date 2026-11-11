using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace FI.AtividadeEntrevista.DAL
{
    internal class AcessoDados
    {
        /// <summary>
        /// Transacao em andamento (opcional). Quando informada, os comandos utilizam
        /// a conexao/transacao dela, que e aberta e fechada pelo chamador.
        /// </summary>
        private readonly Transacao transacao;

        internal AcessoDados(Transacao transacao = null)
        {
            this.transacao = transacao;
        }

        private string stringDeConexao
        {
            get
            {
                ConnectionStringSettings conn = ConfigurationManager.ConnectionStrings["BancoDeDados"];
                if (conn != null)
                    return conn.ConnectionString;
                else
                    return string.Empty;
            }
        }

        internal void Executar(string NomeProcedure, List<SqlParameter> parametros)
        {
            SqlCommand comando = new SqlCommand();
            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandText = NomeProcedure;
            foreach (var item in parametros)
                comando.Parameters.Add(item);

            if (transacao != null)
            {
                comando.Connection = transacao.Conexao;
                comando.Transaction = transacao.TransacaoSql;
                comando.ExecuteNonQuery();
                return;
            }

            SqlConnection conexao = new SqlConnection(stringDeConexao);
            comando.Connection = conexao;
            conexao.Open();
            try
            {
                comando.ExecuteNonQuery();
            }
            finally
            {
                conexao.Close();
            }
        }

        internal DataSet Consultar(string NomeProcedure, List<SqlParameter> parametros)
        {
            SqlCommand comando = new SqlCommand();
            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandText = NomeProcedure;
            foreach (var item in parametros)
                comando.Parameters.Add(item);

            SqlDataAdapter adapter = new SqlDataAdapter(comando);
            DataSet ds = new DataSet();

            if (transacao != null)
            {
                comando.Connection = transacao.Conexao;
                comando.Transaction = transacao.TransacaoSql;
                adapter.Fill(ds);
                return ds;
            }

            SqlConnection conexao = new SqlConnection(stringDeConexao);
            comando.Connection = conexao;
            conexao.Open();

            try
            {
                adapter.Fill(ds);
            }
            finally
            {
                conexao.Close();
            }

            return ds;
        }

    }
}
