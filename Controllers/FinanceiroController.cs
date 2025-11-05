using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parte_do_financeiro.Models;
using Parte_do_financeiro.Context;

namespace Parte_do_financeiro.Controllers
{
    [Authorize]
    public class FinanceiroController : Controller
    {
        private readonly FinanceiroContext _context;  // Alterando de ApplicationDbContext para FinanceiroContext
        private readonly UserManager<IdentityUser> _userManager;

        // Construtor para injeção de dependência
        public FinanceiroController(FinanceiroContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;  // Usando FinanceiroContext
            _userManager = userManager;
        }

        // ===================== LISTAGEM =====================
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            
            // Se o usuário for Administrador ou Síndico, mostra todos os contratos
            if (User.IsInRole("Administrador") || User.IsInRole("Sindico"))
                return View(await _context.Financeiros.Include(f => f.Contrato).ToListAsync());

            // Se for um Morador, mostra apenas os contratos relacionados a ele
#pragma warning disable CS8602 // Desreferência de uma referência possivelmente nula.
            var contratos = await _context.Contratos
                .Where(c => c.LocatarioId == user.Id)
                .Select(c => c.Id)
                .ToListAsync();
#pragma warning restore CS8602 // Desreferência de uma referência possivelmente nula.

            return View(await _context.Financeiros
                .Include(f => f.Contrato)
                .Where(f => contratos.Contains(f.ContratoId))
                .ToListAsync());
        }

        // ===================== CRIAÇÃO =====================
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            // Passando todos os contratos para a view
            ViewBag.Contratos = _context.Contratos.ToList();
            return View();
        }

        [HttpPost, Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create(Financeiro financeiro)
        {
            if (ModelState.IsValid)
            {
                _context.Add(financeiro);  // Adiciona o financeiro no contexto
                await _context.SaveChangesAsync();  // Salva as alterações no banco de dados
                return RedirectToAction(nameof(Index));
            }
            return View(financeiro);
        }

        // ===================== RELATÓRIO =====================
        [Authorize(Roles = "Administrador,Sindico")]
        public async Task<IActionResult> Relatorio()
        {
            var dados = await _context.Financeiros
                .Include(f => f.Contrato)
                .OrderByDescending(f => f.DataPagamento)
                .ToListAsync();

            return View(dados);
        }
    }
}
