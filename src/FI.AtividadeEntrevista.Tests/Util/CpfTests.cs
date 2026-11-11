using FI.AtividadeEntrevista.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FI.AtividadeEntrevista.Tests.Util
{
    [TestClass]
    [TestCategory("Unitario")]
    public class CpfTests
    {
        [DataTestMethod]
        [DataRow("529.982.247-25")]
        [DataRow("52998224725")]
        [DataRow("111.444.777-35")]
        [DataRow("000.000.001-91")]
        [DataRow("000.000.002-72")]
        [DataRow(" 529.982.247-25 ")]
        public void Validar_CpfValido_RetornaVerdadeiro(string cpf)
        {
            Assert.IsTrue(Cpf.Validar(cpf));
        }

        [DataTestMethod]
        [DataRow("529.982.247-24", DisplayName = "segundo dígito verificador errado")]
        [DataRow("529.982.247-15", DisplayName = "primeiro dígito verificador errado")]
        [DataRow("111.111.111-11", DisplayName = "todos os dígitos iguais")]
        [DataRow("000.000.000-00", DisplayName = "zeros")]
        [DataRow("5299822472", DisplayName = "10 dígitos")]
        [DataRow("529982247255", DisplayName = "12 dígitos")]
        [DataRow("abc.def.ghi-jk", DisplayName = "sem dígitos")]
        [DataRow("", DisplayName = "vazio")]
        [DataRow(null, DisplayName = "nulo")]
        public void Validar_CpfInvalido_RetornaFalso(string cpf)
        {
            Assert.IsFalse(Cpf.Validar(cpf));
        }

        [DataTestMethod]
        [DataRow("529.982.247-25", "52998224725")]
        [DataRow(" 529 982 247 25 ", "52998224725")]
        [DataRow("", "")]
        [DataRow(null, "")]
        public void Limpar_RemoveTudoQueNaoForDigito(string entrada, string esperado)
        {
            Assert.AreEqual(esperado, Cpf.Limpar(entrada));
        }

        [DataTestMethod]
        [DataRow("52998224725", "529.982.247-25")]
        [DataRow("529.982.247-25", "529.982.247-25")]
        [DataRow("123", "123", DisplayName = "tamanho inválido é devolvido sem alteração")]
        public void Formatar_AplicaMascaraPadrao(string entrada, string esperado)
        {
            Assert.AreEqual(esperado, Cpf.Formatar(entrada));
        }
    }
}
