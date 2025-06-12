using Microsoft.AspNetCore.Mvc;
using everdadeisso.Models;
using Microsoft.Extensions.Caching.Memory;
using everdadeisso.Models.ViewModels;
using everdadeisso.Interfaces;

namespace everdadeisso.Controllers
{
    public class VerificarController : Controller
    {
        private readonly IVerificacaoService _verificacaoService;
        private readonly OpenAIService _openai;
        private readonly IMemoryCache _cache;

        public VerificarController(IVerificacaoService verificacaoService, OpenAIService openai, IMemoryCache cache)
        {
            _verificacaoService = verificacaoService;
            _openai = openai;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (perguntas, dica) = await _openai.GerarSugestoesEDicasAsync();

            ViewBag.Perguntas = perguntas;
            ViewBag.Dica = dica;

            return View();
        }

        [HttpPost]
        public IActionResult Index(string texto)
        {
            var id = Guid.NewGuid().ToString("N").Substring(0, 8);

            var pendente = new ResultadoViewModel
            {
                Enviado = texto,
                Classificacao = "pendente",
                Explicacao = "<p>Estamos verificando sua pergunta...</p>",
                Referencias = new List<Referencia>()
            };

            _cache.Set("verificacao_" + id, pendente, TimeSpan.FromMinutes(5));

            _ = Task.Run(async () =>
            {
                try
                {
                    var resultado = await _verificacaoService.VerificarConteudoAsync(texto);
                    _cache.Set("verificacao_" + id, resultado, TimeSpan.FromMinutes(5));
                }
                catch
                {
                    var erro = new ResultadoViewModel
                    {
                        Enviado = texto,
                        Classificacao = "erro",
                        Explicacao = "<p>Ocorreu um erro ao verificar a informação. Tente novamente.</p>",
                        Referencias = new List<Referencia>()
                    };
                    _cache.Set("verificacao_" + id, erro, TimeSpan.FromMinutes(5));
                }
            });

            return RedirectToAction("Resultado", new { id });
        }

        [HttpGet]
        public IActionResult Resultado(string id)
        {
            var resultado = _cache.Get<ResultadoViewModel>("verificacao_" + id);
            if (resultado == null)
            {
                return RedirectToAction("Index");
            }
            return View(resultado);
        }

        [HttpGet]
        public IActionResult VerificarStatus(string id)
        {
            var resultado = _cache.Get<ResultadoViewModel>("verificacao_" + id);
            if (resultado == null)
                return Json(new { status = "não encontrado" });

            return Json(new
            {
                status = resultado.Classificacao,
                enviado = resultado.Enviado,
                explicacao = resultado.Explicacao,
                referencias = resultado.Referencias
            });
        }
    }
}