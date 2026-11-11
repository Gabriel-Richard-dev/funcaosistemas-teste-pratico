using FI.AtividadeEntrevista.BLL;
using FI.AtividadeEntrevista.Tests.Infra;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FI.AtividadeEntrevista.Tests.BLL
{
    /// <summary>
    /// Regras validadas pelo BoCliente ANTES de qualquer acesso ao banco
    /// (por isso nao dependem do LocalDB)
    /// </summary>
    [TestClass]
    [TestCategory("Unitario")]
    public class BoClienteValidacaoTests
    {
        private readonly BoCliente bo = new BoCliente();

        [TestMethod]
        public void Incluir_CpfDoClienteInvalido_LancaRegraNegocio()
        {
            var ex = Assert.ThrowsException<RegraNegocioException>(() => bo.Incluir(Fabrica.Cliente("529.982.247-24")));
            StringAssert.Contains(ex.Message, "CPF do cliente inválido");
        }

        [TestMethod]
        public void Incluir_CpfDoClienteVazio_LancaRegraNegocio()
        {
            Assert.ThrowsException<RegraNegocioException>(() => bo.Incluir(Fabrica.Cliente("")));
        }

        [TestMethod]
        public void Alterar_CpfDoClienteInvalido_LancaRegraNegocio()
        {
            var cliente = Fabrica.Cliente("111.111.111-11");
            cliente.Id = 1;
            Assert.ThrowsException<RegraNegocioException>(() => bo.Alterar(cliente));
        }

        [TestMethod]
        public void Incluir_BeneficiarioComCpfInvalido_LancaRegraNegocio()
        {
            var cliente = Fabrica.Cliente(Fabrica.CpfA, Fabrica.Beneficiario("000.000.001-90", "Maria"));

            var ex = Assert.ThrowsException<RegraNegocioException>(() => bo.Incluir(cliente));
            StringAssert.Contains(ex.Message, "Maria");
        }

        [TestMethod]
        public void Incluir_DoisBeneficiariosComMesmoCpf_LancaRegraNegocio()
        {
            // mesmo CPF com e sem mascara deve ser considerado duplicado
            var cliente = Fabrica.Cliente(Fabrica.CpfA,
                Fabrica.Beneficiario(Fabrica.Benef1, "Maria"),
                Fabrica.Beneficiario("00000000191", "José"));

            var ex = Assert.ThrowsException<RegraNegocioException>(() => bo.Incluir(cliente));
            StringAssert.Contains(ex.Message, "000.000.001-91");
        }

        [TestMethod]
        public void Incluir_BeneficiarioSemNome_LancaRegraNegocio()
        {
            var cliente = Fabrica.Cliente(Fabrica.CpfA, Fabrica.Beneficiario(Fabrica.Benef1, "   "));
            Assert.ThrowsException<RegraNegocioException>(() => bo.Incluir(cliente));
        }

        [TestMethod]
        public void Incluir_BeneficiarioComNomeMaiorQue50_LancaRegraNegocio()
        {
            var cliente = Fabrica.Cliente(Fabrica.CpfA, Fabrica.Beneficiario(Fabrica.Benef1, new string('a', 51)));
            Assert.ThrowsException<RegraNegocioException>(() => bo.Incluir(cliente));
        }
    }
}
