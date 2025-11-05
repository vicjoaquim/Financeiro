using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Parte_do_financeiro.Context;

var builder = WebApplication.CreateBuilder(args);

// Adiciona os serviços de MVC
builder.Services.AddControllersWithViews();

// Adiciona os serviços de autenticação e autorização
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // Certifique-se de que a autenticação está ativada
app.UseAuthorization();  // Autorize as rotas de acordo com os papéis

// Defina a rota padrão para a página de login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");  // Redireciona para a ação Login do AccountController

app.Run();

