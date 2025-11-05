using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Parte_do_financeiro.Models
{
    public class Contrato
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Precision(18, 2)]
        public decimal ValorMensal { get; set; }

        [Precision(5, 2)]
        public decimal ReajustePercentual { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        [Required]
        public string LocatarioId { get; set; } = string.Empty;

        [ForeignKey("LocatarioId")]
        public IdentityUser Locatario { get; set; } = null!;

        public string Status { get; set; } = "Ativo";
        public string Observacoes { get; set; } = string.Empty;
    }
}
