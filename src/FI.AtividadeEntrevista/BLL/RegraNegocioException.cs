using System;

namespace FI.AtividadeEntrevista.BLL
{
    /// <summary>
    /// Excecao lancada quando uma regra de negocio e violada.
    /// A mensagem e destinada ao usuario final.
    /// </summary>
    public class RegraNegocioException : Exception
    {
        public RegraNegocioException(string mensagem) : base(mensagem)
        {
        }
    }
}
