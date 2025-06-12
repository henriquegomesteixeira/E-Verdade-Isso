using everdadeisso.Models.ViewModels;

namespace everdadeisso.Interfaces
{
    public interface IVerificacaoService
    {
        Task<ResultadoViewModel> VerificarConteudoAsync(string texto);
    }
}
