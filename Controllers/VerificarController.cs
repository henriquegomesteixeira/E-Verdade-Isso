using everdadeisso.Service;
using Microsoft.AspNetCore.Mvc;
using Markdig;
using everdadeisso.Models;

namespace everdadeisso.Controllers
{
    public class VerificarController : Controller
    {
        private readonly PerplexityService _perplexity;
        private readonly OpenAIService _openai;

        public VerificarController(PerplexityService perplexity, OpenAIService openai)
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
            (string classificacao, string explicacaoHtml, List<Referencia> referencias) = await _perplexity.VerificarNoticiaAsync(texto);

            ViewBag.Enviado = texto;
            ViewBag.Classificacao = classificacao;
            ViewBag.ExplicacaoHtml = explicacaoHtml;
            ViewBag.Referencias = referencias;

            return View();
        }
    }
}
