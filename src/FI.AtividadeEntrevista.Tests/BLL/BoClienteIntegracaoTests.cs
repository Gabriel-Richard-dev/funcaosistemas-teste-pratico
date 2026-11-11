using System.Data.SqlClient;
using System.Linq;
using FI.AtividadeEntrevista.BLL;
using FI.AtividadeEntrevista.Tests.Infra;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FI.AtividadeEntrevista.Tests.BLL
{
    /// <summary>
    /// Testes de integracao do BoCliente com o banco (copia temporaria do BancoDeDados.mdf)
    /// </summary>
    [TestClass]
    [TestCategory("Integracao")]
    public class BoClienteIntegracaoTests
    {
        private BoCliente bo;

        [TestInitialize]
        public void Preparar()
        {
            BancoDeTestes.GarantirDisponivel();
            BancoDeTestes.LimparTabelas();
            bo = new BoCliente();
        }

        // ---------- CPF do cliente ----------

        [TestMethod]
        public void Incluir_ClienteValido_GravaCpfSomenteComDigitos()
        {
            long id = bo.Incluir(Fabrica.Cliente(Fabrica.CpfA));

            Assert.IsTrue(id > 0);
            Assert.AreEqual("52998224725", bo.Consultar(id).CPF);
        }

        [TestMethod]
        public void Incluir_CpfJaCadastrado_LancaRegraNegocioENaoGrava()
        {
            bo.Incluir(Fabrica.Cliente(Fabrica.CpfA));

            // mesmo CPF sem mascara
            var ex = Assert.ThrowsException<RegraNegocioException>(() => bo.Incluir(Fabrica.Cliente("52998224725")));

            StringAssert.Contains(ex.Message, "Já existe um cliente");
            Assert.AreEqual(1, BancoDeTestes.Contar("CLIENTES"));
        }

        [TestMethod]
        public void Alterar_MantendoOProprioCpf_Permite()
        {
            long id = bo.Incluir(Fabrica.Cliente(Fabrica.CpfA));
            var cliente = bo.Consultar(id);
            cliente.Nome = "João Alterado";

            bo.Alterar(cliente);

            Assert.AreEqual("João Alterado", bo.Consultar(id).Nome);
        }

        [TestMethod]
        public void Alterar_ParaCpfDeOutroCliente_LancaRegraNegocioENaoAltera()
        {
            bo.Incluir(Fabrica.Cliente(Fabrica.CpfA));
            long idB = bo.Incluir(Fabrica.Cliente(Fabrica.CpfB));

            var clienteB = bo.Consultar(idB);
            clienteB.CPF = Fabrica.CpfA;

            Assert.ThrowsException<RegraNegocioException>(() => bo.Alterar(clienteB));
            Assert.AreEqual("11144477735", bo.Consultar(idB).CPF);
        }

        [TestMethod]
        public void VerificarExistencia_IgnoraOProprioCliente()
        {
            long id = bo.Incluir(Fabrica.Cliente(Fabrica.CpfA));

            Assert.IsTrue(bo.VerificarExistencia(Fabrica.CpfA));
            Assert.IsFalse(bo.VerificarExistencia(Fabrica.CpfA, id));
            Assert.IsFalse(bo.VerificarExistencia(Fabrica.CpfB));
        }

        // ---------- Beneficiarios ----------

        [TestMethod]
        public void Incluir_ComBeneficiarios_GravaTodosJuntoComOCliente()
        {
            long id = bo.Incluir(Fabrica.Cliente(Fabrica.CpfA,
                Fabrica.Beneficiario(Fabrica.Benef1, "Beneficiário João"),
                Fabrica.Beneficiario(Fabrica.Benef2, "Beneficiário José")));

            var beneficiarios = bo.Consultar(id).Beneficiarios;

            Assert.AreEqual(2, beneficiarios.Count);
            Assert.IsTrue(beneficiarios.All(b => b.IdCliente == id && b.Id > 0));
            CollectionAssert.AreEquivalent(new[] { "00000000191", "00000000272" }, beneficiarios.Select(b => b.CPF).ToArray());
        }

        [TestMethod]
        public void Incluir_MesmoCpfDeBeneficiarioEmClientesDiferentes_Permite()
        {
            bo.Incluir(Fabrica.Cliente(Fabrica.CpfA, Fabrica.Beneficiario(Fabrica.Benef1, "Maria")));
            bo.Incluir(Fabrica.Cliente(Fabrica.CpfB, Fabrica.Beneficiario(Fabrica.Benef1, "Maria")));

            Assert.AreEqual(2, BancoDeTestes.Contar("BENEFICIARIOS"));
        }

        [TestMethod]
        public void Alterar_SincronizaBeneficiarios_AlteraExcluiEInclui()
        {
            long id = bo.Incluir(Fabrica.Cliente(Fabrica.CpfA,
                Fabrica.Beneficiario(Fabrica.Benef1, "João"),
                Fabrica.Beneficiario(Fabrica.Benef2, "José")));

            var cliente = bo.Consultar(id);
            var joao = cliente.Beneficiarios.Single(b => b.Nome == "João");
            joao.Nome = "João Alterado";
            joao.CPF = Fabrica.Benef3;
            cliente.Beneficiarios.RemoveAll(b => b.Nome == "José");
            cliente.Beneficiarios.Add(Fabrica.Beneficiario(Fabrica.Benef4, "Novo"));

            bo.Alterar(cliente);

            var depois = bo.Consultar(id).Beneficiarios;
            Assert.AreEqual(2, depois.Count);

            var joaoDepois = depois.Single(b => b.Id == joao.Id);   // mesmo registro, alterado
            Assert.AreEqual("João Alterado", joaoDepois.Nome);
            Assert.AreEqual("00000000353", joaoDepois.CPF);

            Assert.IsTrue(depois.Any(b => b.Nome == "Novo" && b.CPF == "00000000434"));
            Assert.IsFalse(depois.Any(b => b.Nome == "José"));
        }

        [TestMethod]
        public void Alterar_SemBeneficiarios_ExcluiTodos()
        {
            long id = bo.Incluir(Fabrica.Cliente(Fabrica.CpfA, Fabrica.Beneficiario(Fabrica.Benef1, "Maria")));

            var cliente = bo.Consultar(id);
            cliente.Beneficiarios.Clear();
            bo.Alterar(cliente);

            Assert.AreEqual(0, bo.Consultar(id).Beneficiarios.Count);
        }

        [TestMethod]
        public void Alterar_IdDeBeneficiarioDeOutroCliente_NaoAlteraORegistroDoOutroCliente()
        {
            long idA = bo.Incluir(Fabrica.Cliente(Fabrica.CpfA, Fabrica.Beneficiario(Fabrica.Benef1, "Do cliente A")));
            long idB = bo.Incluir(Fabrica.Cliente(Fabrica.CpfB));
            var beneficiarioDeA = bo.Consultar(idA).Beneficiarios.Single();

            var clienteB = bo.Consultar(idB);
            clienteB.Beneficiarios.Add(Fabrica.Beneficiario(Fabrica.Benef2, "Tentativa", beneficiarioDeA.Id));
            bo.Alterar(clienteB);

            var aDepois = bo.Consultar(idA).Beneficiarios.Single();
            Assert.AreEqual("Do cliente A", aDepois.Nome);
            Assert.AreEqual("00000000191", aDepois.CPF);
        }

        // ---------- Transacao ----------

        [TestMethod]
        public void Incluir_FalhaAoGravarBeneficiario_DesfazTambemOCliente()
        {
            // CHECK temporario faz o INSERT do beneficiario falhar depois que o cliente ja foi gravado
            BancoDeTestes.Executar("ALTER TABLE BENEFICIARIOS ADD CONSTRAINT CK_TESTE_FALHA CHECK (NOME <> 'FALHA')");
            try
            {
                var cliente = Fabrica.Cliente(Fabrica.CpfC, Fabrica.Beneficiario(Fabrica.Benef1, "FALHA"));

                Assert.ThrowsException<SqlException>(() => bo.Incluir(cliente));

                Assert.IsFalse(bo.VerificarExistencia(Fabrica.CpfC), "O cliente deveria ter sido desfeito junto com o beneficiário.");
                Assert.AreEqual(0, BancoDeTestes.Contar("CLIENTES"));
            }
            finally
            {
                BancoDeTestes.Executar("ALTER TABLE BENEFICIARIOS DROP CONSTRAINT CK_TESTE_FALHA");
            }
        }

        // ---------- Exclusao ----------

        [TestMethod]
        public void Excluir_RemoveClienteESeusBeneficiarios()
        {
            long id = bo.Incluir(Fabrica.Cliente(Fabrica.CpfA, Fabrica.Beneficiario(Fabrica.Benef1, "Maria")));

            bo.Excluir(id);

            Assert.IsNull(bo.Consultar(id));
            Assert.AreEqual(0, BancoDeTestes.Contar("BENEFICIARIOS"));
        }

        [TestMethod]
        public void Listar_RetornaClientesComCpf()
        {
            bo.Incluir(Fabrica.Cliente(Fabrica.CpfA));
            bo.Incluir(Fabrica.Cliente(Fabrica.CpfB));

            var clientes = bo.Listar();

            Assert.AreEqual(2, clientes.Count);
            Assert.IsTrue(clientes.All(c => c.CPF.Length == 11));
        }
    }
}
