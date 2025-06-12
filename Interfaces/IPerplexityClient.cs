using everdadeisso.Models.DTOs;

namespace everdadeisso.Interfaces
{
    public interface IPerplexityClient
    {
        Task<RespostaDto> ObterRespostaDaPerplexityAsync(string texto);
    }
}
