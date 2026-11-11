using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FI.AtividadeEntrevista.Tests.Infra
{
    /// <summary>
    /// Prepara o banco exclusivo dos testes de integracao.
    ///
    /// Quando a connection string usa AttachDbFilename (padrao, LocalDB), uma COPIA do
    /// BancoDeDados.mdf da aplicacao e colocada em uma pasta temporaria e anexada com o nome
    /// definido em "Initial Catalog". Ao final da execucao o banco e desanexado e a pasta removida.
    ///
    /// Se o banco nao estiver disponivel (ex.: LocalDB nao instalado), os testes de integracao
    /// ficam como "Inconclusivo" em vez de falhar.
    /// </summary>
    [TestClass]
    public class BancoDeTestes
    {
        private static string pastaTemporaria;
        private static string motivoIndisponivel;

        public static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["BancoDeDados"].ConnectionString; }
        }

        [AssemblyInitialize]
        public static void Inicializar(TestContext contexto)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);

                // Trava de seguranca: nunca executar a limpeza das tabelas em um banco que nao seja de testes
                if (builder.InitialCatalog.IndexOf("Teste", StringComparison.OrdinalIgnoreCase) < 0)
                    throw new InvalidOperationException("O 'Initial Catalog' da connection string de testes deve conter 'Teste'.");

                if (!string.IsNullOrEmpty(builder.AttachDBFilename))
                {
                    pastaTemporaria = Path.Combine(Path.GetTempPath(), "FI.AtividadeEntrevista.Tests");
                    Desanexar(builder);
                    RecriarPasta(pastaTemporaria);

                    string origem = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Banco");
                    File.Copy(Path.Combine(origem, "BancoDeDados.mdf"), Path.Combine(pastaTemporaria, "BancoDeDados.mdf"));
                    File.Copy(Path.Combine(origem, "BancoDeDados_log.ldf"), Path.Combine(pastaTemporaria, "BancoDeDados_log.ldf"));

                    AppDomain.CurrentDomain.SetData("DataDirectory", pastaTemporaria);
                }

                using (SqlConnection conexao = new SqlConnection(ConnectionString))
                    conexao.Open();
            }
            catch (Exception ex)
            {
                motivoIndisponivel = "Banco de testes indisponível (LocalDB instalado?): " + ex.Message;
            }
        }

        [AssemblyCleanup]
        public static void Finalizar()
        {
            if (pastaTemporaria == null)
                return;

            try
            {
                SqlConnection.ClearAllPools();
                Desanexar(new SqlConnectionStringBuilder(ConnectionString));
                Directory.Delete(pastaTemporaria, true);
            }
            catch
            {
                // limpeza "best effort": a proxima execucao desanexa e recria a pasta
            }
        }

        /// <summary>
        /// Marca o teste como inconclusivo se o banco nao estiver disponivel
        /// </summary>
        public static void GarantirDisponivel()
        {
            if (motivoIndisponivel != null)
                Assert.Inconclusive(motivoIndisponivel);
        }

        /// <summary>
        /// Remove todos os registros, deixando cada teste independente dos demais
        /// </summary>
        public static void LimparTabelas()
        {
            Executar("DELETE FROM BENEFICIARIOS; DELETE FROM CLIENTES;");
        }

        public static void Executar(string sql)
        {
            using (SqlConnection conexao = new SqlConnection(ConnectionString))
            using (SqlCommand comando = new SqlCommand(sql, conexao))
            {
                conexao.Open();
                comando.ExecuteNonQuery();
            }
        }

        public static int Contar(string tabela)
        {
            using (SqlConnection conexao = new SqlConnection(ConnectionString))
            using (SqlCommand comando = new SqlCommand("SELECT COUNT(*) FROM " + tabela, conexao))
            {
                conexao.Open();
                return (int)comando.ExecuteScalar();
            }
        }

        private static void Desanexar(SqlConnectionStringBuilder builder)
        {
            SqlConnectionStringBuilder master = new SqlConnectionStringBuilder(builder.ConnectionString)
            {
                AttachDBFilename = string.Empty,
                InitialCatalog = "master"
            };

            using (SqlConnection conexao = new SqlConnection(master.ConnectionString))
            using (SqlCommand comando = new SqlCommand(
                @"IF DB_ID(@nome) IS NOT NULL
                  BEGIN
                      EXEC('ALTER DATABASE [' + @nome + '] SET SINGLE_USER WITH ROLLBACK IMMEDIATE');
                      EXEC sp_detach_db @nome;
                  END", conexao))
            {
                comando.Parameters.AddWithValue("@nome", builder.InitialCatalog);
                conexao.Open();
                comando.ExecuteNonQuery();
            }
        }

        private static void RecriarPasta(string pasta)
        {
            if (Directory.Exists(pasta))
                Directory.Delete(pasta, true);

            Directory.CreateDirectory(pasta);
        }
    }
}
