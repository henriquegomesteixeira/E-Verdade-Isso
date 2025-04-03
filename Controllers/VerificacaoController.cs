using everdadeisso.Service;
using Microsoft.AspNetCore.Mvc;
using Markdig;

namespace everdadeisso.Controllers
{
    public class VerificacaoController : Controller
    {
        private readonly PerplexityService _perplexity;

        public VerificacaoController(PerplexityService perplexity)
        {
            _perplexity = perplexity;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string texto)
        {
            var resultado = await _perplexity.VerificarNoticiaAsync(texto);
            var html = Markdown.ToHtml(resultado);
            ViewBag.Resultado = html;
            return View();
        }
    }
}
