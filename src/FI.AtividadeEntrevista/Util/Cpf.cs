using System.Linq;

namespace FI.AtividadeEntrevista.Util
{
    /// <summary>
    /// Utilitarios para tratamento e validacao de CPF
    /// </summary>
    public static class Cpf
    {
        /// <summary>
        /// Remove a mascara do CPF, mantendo somente os digitos
        /// </summary>
        public static string Limpar(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return string.Empty;

            return new string(cpf.Where(char.IsDigit).ToArray());
        }

        /// <summary>
        /// Aplica a mascara padrao de CPF (999.999.999-99)
        /// </summary>
        public static string Formatar(string cpf)
        {
            string digitos = Limpar(cpf);
            if (digitos.Length != 11)
                return cpf;

            return string.Format("{0}.{1}.{2}-{3}",
                digitos.Substring(0, 3), digitos.Substring(3, 3), digitos.Substring(6, 3), digitos.Substring(9, 2));
        }

        /// <summary>
        /// Valida o CPF conforme o calculo dos digitos verificadores.
        /// Aceita o CPF com ou sem mascara.
        /// </summary>
        public static bool Validar(string cpf)
        {
            string digitos = Limpar(cpf);

            if (digitos.Length != 11)
                return false;

            // CPFs com todos os digitos iguais (ex.: 111.111.111-11) passam no calculo, mas sao invalidos
            if (digitos.Distinct().Count() == 1)
                return false;

            int[] numeros = digitos.Select(c => c - '0').ToArray();

            return numeros[9] == CalcularDigito(numeros, 9)
                && numeros[10] == CalcularDigito(numeros, 10);
        }

        /// <summary>
        /// Calcula o digito verificador a partir das primeiras <paramref name="quantidade"/> posicoes,
        /// com pesos decrescentes iniciando em (quantidade + 1)
        /// </summary>
        private static int CalcularDigito(int[] numeros, int quantidade)
        {
            int soma = 0;
            for (int i = 0; i < quantidade; i++)
                soma += numeros[i] * (quantidade + 1 - i);

            int resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }
    }
}
