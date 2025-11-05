using Microsoft.AspNetCore.Mvc;

namespace Parte_do_financeiro.Controllers
{
    public class ComunicacaoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
