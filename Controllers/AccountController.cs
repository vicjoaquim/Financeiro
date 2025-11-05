using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Parte_do_financeiro.Controllers
{
    public class AccountController : Controller
    {
        // ============================
        // INJEÇÃO DE DEPENDÊNCIAS
        // ============================
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // 🔹 ÚNICO construtor permitido (NÃO pode haver outro!)
        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ============================
        // LOGIN
        // ============================
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Preencha todos os campos.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    ViewBag.Error = "Usuário não encontrado.";
                    return View();
                }

                var roles = await _userManager.GetRolesAsync(user);

                // 🔹 Redirecionamento conforme papel
                if (roles.Contains("Administrador"))
                    return RedirectToAction("Index", "Home"); // Se for Administrador, redireciona para o Home

                if (roles.Contains("Sindico"))
                    return RedirectToAction("Index", "Financeiro"); // Redireciona para a área do Síndico

                if (roles.Contains("Morador"))
                    return RedirectToAction("Index", "Contratos"); // Redireciona para a área do Morador

                // Se não tiver papel definido, desloga o usuário
                await _signInManager.SignOutAsync();
                ViewBag.Error = "Usuário sem papel definido. Contate o administrador.";
                return View();
            }

            ViewBag.Error = "E-mail ou senha incorretos.";
            return View();
        }


        // ============================
        // CADASTRO (somente ADMIN)
        // ============================
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Roles = new List<string> { "Administrador", "Sindico", "Morador" };
            return View();
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                ViewBag.Error = "Preencha todos os campos.";
                ViewBag.Roles = new List<string> { "Administrador", "Sindico", "Morador" };
                return View();
            }

            // Verifica se o papel existe
            if (!await _roleManager.RoleExistsAsync(role))
            {
                ViewBag.Error = $"O papel {role} não existe.";
                ViewBag.Roles = new List<string> { "Administrador", "Sindico", "Morador" };
                return View();
            }

            // Cria o novo usuário
            var user = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);

                // Morador não faz login automático
                if (role == "Morador")
                {
                    TempData["Success"] = "Morador criado com sucesso!";
                    return RedirectToAction("Index", "Home");
                }

                // Admin e Síndico logam direto
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Home");
            }

            // Mostra erros
            ViewBag.Error = string.Join(", ", result.Errors.Select(e => e.Description));
            ViewBag.Roles = new List<string> { "Administrador", "Sindico", "Morador" };
            return View();
        }

        // ============================
        // LOGOUT
        // ============================
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // ============================
        // ACESSO NEGADO
        // ============================
        public IActionResult AccessDenied() => View();
    }
}
