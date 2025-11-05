using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Parte_do_financeiro.Models;

namespace Parte_do_financeiro.Context
{
    public class FinanceiroContext : DbContext
{
    public DbSet<Contrato> Contratos { get; set; }
    public DbSet<Financeiro> Financeiros { get; set; }

        public FinanceiroContext(DbContextOptions<FinanceiroContext> options)
            : base(options) { }
        
        
}
}