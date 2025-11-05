using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Parte_do_financeiro.Models
{
    public class Financeiro
    {
         public int Id { get; set; }
    [Precision(18, 2)]
    public decimal ValorTotal { get; set; }
    public DateTime DataPagamento { get; set; }

    public int ContratoId { get; set; }
    public Contrato Contrato { get; set; } = null!;

    public string StatusPagamento { get; set; } = "Pendente";
    public string Descricao { get; set; } = string.Empty;
    }
}
