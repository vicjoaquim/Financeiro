using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parte_do_financeiro.Models;
using Parte_do_financeiro.Context;

namespace Parte_do_financeiro.Controllers
{
    [Authorize]
    public class ContratosController : Controller
    {
        private readonly FinanceiroContext _context;  // Utilizando FinanceiroContext
        private readonly UserManager<IdentityUser> _userManager;

        public ContratosController(FinanceiroContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;  // Injeção do FinanceiroContext
            _userManager = userManager;
        }

        // ===================== LISTAGEM =====================
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            // Se o usuário for Administrador ou Síndico, mostra todos os contratos
            if (User.IsInRole("Administrador") || User.IsInRole("Sindico"))
                return View(await _context.Contratos.Include(c => c.Locatario).ToListAsync());

            // Se for um Morador, mostra apenas os contratos relacionados a ele
            var meus = await _context.Contratos
                .Include(c => c.Locatario)
                .Where(c => c.LocatarioId == user!.Id)
                .ToListAsync();

            return View(meus);
        }

        // ===================== CRIAÇÃO =====================
        [Authorize(Roles = "Administrador,Sindico")]
        public async Task<IActionResult> Create()
        {
            // 🔹 Corrigido: busca síncrona via loop para evitar erro async
            var usuariosMoradores = new List<IdentityUser>();
            foreach (var u in _userManager.Users)
            {
                if (await _userManager.IsInRoleAsync(u, "Morador"))
                    usuariosMoradores.Add(u);
            }

            ViewBag.Usuarios = usuariosMoradores;
            return View();
        }

        [HttpPost, Authorize(Roles = "Administrador,Sindico")]
        public async Task<IActionResult> Create(Contrato contrato)
        {
            if (ModelState.IsValid)
            {
                // 🔹 Valida o Locatário
                var locatario = await _userManager.FindByIdAsync(contrato.LocatarioId);
                if (locatario == null)
                {
                    ModelState.AddModelError("LocatarioId", "Selecione um locatário válido.");
                    await CarregarUsuariosMoradoresAsync();
                    return View(contrato);
                }

                _context.Add(contrato);  // Adiciona o contrato ao contexto
                await _context.SaveChangesAsync();  // Salva no banco de dados SQL Server

                return RedirectToAction(nameof(Index));
            }

            await CarregarUsuariosMoradoresAsync();
            return View(contrato);
        }

        // ===================== EDIÇÃO =====================
        [Authorize(Roles = "Administrador,Sindico")]
        public async Task<IActionResult> Edit(int id)
        {
            var contrato = await _context.Contratos.FindAsync(id);
            if (contrato == null) return NotFound();

            await CarregarUsuariosMoradoresAsync();
            return View(contrato);
        }

        [HttpPost, Authorize(Roles = "Administrador,Sindico")]
        public async Task<IActionResult> Edit(int id, Contrato contrato)
        {
            if (id != contrato.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contrato);  // Atualiza o contrato no contexto
                    await _context.SaveChangesAsync();  // Salva as alterações no banco
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Contratos.Any(e => e.Id == contrato.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await CarregarUsuariosMoradoresAsync();
            return View(contrato);
        }

        // ===================== DETALHES =====================
        public async Task<IActionResult> Details(int id)
        {
            var contrato = await _context.Contratos
                .Include(c => c.Locatario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contrato == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Morador") && contrato.LocatarioId != user!.Id)
                return Forbid();

            return View(contrato);
        }

        // ===================== EXCLUSÃO =====================
        [Authorize(Roles = "Administrador,Sindico")]
        public async Task<IActionResult> Delete(int id)
        {
            var contrato = await _context.Contratos.FindAsync(id);
            if (contrato == null) return NotFound();

            _context.Contratos.Remove(contrato);  // Remove o contrato do contexto
            await _context.SaveChangesAsync();  // Salva a exclusão no banco

            return RedirectToAction(nameof(Index));
        }

        // ===================== MÉTODO AUXILIAR =====================
        private async Task CarregarUsuariosMoradoresAsync()
        {
            var usuariosMoradores = new List<IdentityUser>();
            foreach (var u in _userManager.Users)
            {
                if (await _userManager.IsInRoleAsync(u, "Morador"))
                    usuariosMoradores.Add(u);
            }

            ViewBag.Usuarios = usuariosMoradores;
        }
    }
}
