using System.Diagnostics;
using everdadeisso.Models;
using Microsoft.AspNetCore.Mvc;

namespace everdadeisso.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("privacidade")]
        public IActionResult Privacidade()
        {
            return View();
        }

        [Route("termos")]
        public IActionResult Termos()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
