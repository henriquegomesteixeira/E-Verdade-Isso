using everdadeisso.Service;
using Microsoft.AspNetCore.Mvc;
using Markdig;

namespace everdadeisso.Controllers
{
    public class VerificacaoController : Controller
    {
        private readonly PerplexityService _perplexity;
        private readonly OpenAIService _openai;

        public VerificacaoController(PerplexityService perplexity, OpenAIService openai)
        {
            _perplexity = perplexity;
            _openai = openai;
        }

        public async Task<IActionResult> Index()
        {
            var (perguntas, dica) = await _openai.GerarSugestoesEDicasAsync();

            ViewBag.Perguntas = perguntas;
            ViewBag.Dica = dica;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string texto)
        {
            (string classificacao, string explicacaoHtml, string referenciasHtml) = await _perplexity.VerificarNoticiaAsync(texto);

            ViewBag.Enviado = texto;
            ViewBag.Classificacao = classificacao;
            ViewBag.ExplicacaoHtml = explicacaoHtml;
            ViewBag.ReferenciasHtml = referenciasHtml;

            return View();
        }
    }
}
