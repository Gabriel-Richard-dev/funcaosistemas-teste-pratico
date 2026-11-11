using System.ComponentModel.DataAnnotations;

namespace WebAtividadeEntrevista.Models
{
    /// <summary>
    /// Classe de Modelo de Beneficiario
    /// </summary>
    public class BeneficiarioModel
    {
        public long Id { get; set; }

        /// <summary>
        /// CPF
        /// </summary>
        [Required(ErrorMessage = "O CPF do beneficiário é obrigatório")]
        [Cpf(ErrorMessage = "Digite um CPF de beneficiário válido")]
        public string CPF { get; set; }

        /// <summary>
        /// Nome
        /// </summary>
        [Required(ErrorMessage = "O nome do beneficiário é obrigatório")]
        [MaxLength(50, ErrorMessage = "O nome do beneficiário deve possuir no máximo 50 caracteres")]
        public string Nome { get; set; }
    }
}
