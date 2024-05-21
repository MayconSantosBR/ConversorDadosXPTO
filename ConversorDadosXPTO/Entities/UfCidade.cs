using System;
using System.Collections.Generic;

namespace ConversorDadosXPTO.Models;

public partial class UfCidade
{
    public int IdufCidade { get; set; }

    public string? Uf { get; set; }

    public string? Cidade { get; set; }

    public virtual ICollection<Dado> Dados { get; set; } = new List<Dado>();
}
