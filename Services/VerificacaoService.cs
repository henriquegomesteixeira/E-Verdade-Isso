using everdadeisso.Interfaces;
using everdadeisso.Mappers;
using everdadeisso.Models.ViewModels;

public class VerificacaoService : IVerificacaoService
{
    private readonly IPerplexityClient _perplexityClient;

    public VerificacaoService(IPerplexityClient client)
    {
        _perplexityClient = client;
    }

    public async Task<ResultadoViewModel> VerificarConteudoAsync(string texto)
    {
        var resposta = await _perplexityClient.ObterRespostaDaPerplexityAsync(texto);

        return new ResultadoViewModel
        {
            Enviado = texto,
            Classificacao = resposta.Classificacao,
            Explicacao = RespostaIaMapper.LimparMarcadoresDeReferencia(resposta.Explicacao),
            Referencias = RespostaIaMapper.FormatarReferenciasComDetalhes(resposta.Referencias)
        };
    }
}
