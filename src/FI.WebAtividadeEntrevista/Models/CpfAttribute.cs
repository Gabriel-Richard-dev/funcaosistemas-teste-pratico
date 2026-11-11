using System;
using System.ComponentModel.DataAnnotations;

namespace WebAtividadeEntrevista.Models
{
    /// <summary>
    /// Valida se o valor informado e um CPF valido (digitos verificadores).
    /// Valores vazios sao ignorados; para obrigatoriedade utilize [Required].
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class CpfAttribute : ValidationAttribute
    {
        public CpfAttribute() : base("Digite um CPF válido")
        {
        }

        public override bool IsValid(object value)
        {
            string cpf = value as string;

            if (string.IsNullOrWhiteSpace(cpf))
                return true;

            return FI.AtividadeEntrevista.Util.Cpf.Validar(cpf);
        }
    }
}
