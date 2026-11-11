using System.Linq;
using FI.AtividadeEntrevista.DML;

namespace FI.AtividadeEntrevista.Tests.Infra
{
    /// <summary>
    /// Fabrica de objetos e CPFs validos para os testes
    /// </summary>
    internal static class Fabrica
    {
        // CPFs validos (digitos verificadores conferidos)
        public const string CpfA = "529.982.247-25";
        public const string CpfB = "111.444.777-35";
        public const string CpfC = "390.533.447-05";
        public const string Benef1 = "000.000.001-91";
        public const string Benef2 = "000.000.002-72";
        public const string Benef3 = "000.000.003-53";
        public const string Benef4 = "000.000.004-34";

        public static Cliente Cliente(string cpf, params Beneficiario[] beneficiarios)
        {
            return new Cliente()
            {
                Nome = "João",
                Sobrenome = "da Silva",
                Nacionalidade = "brasileira",
                CEP = "01011-100",
                Estado = "SP",
                Cidade = "São Paulo",
                Logradouro = "Rua Boa Vista 253",
                Email = "joao@email.com",
                Telefone = "(11) 2020-3030",
                CPF = cpf,
                Beneficiarios = beneficiarios.ToList()
            };
        }

        public static Beneficiario Beneficiario(string cpf, string nome, long id = 0)
        {
            return new Beneficiario() { Id = id, CPF = cpf, Nome = nome };
        }
    }
}
