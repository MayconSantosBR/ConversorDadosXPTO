using System;
using System.Collections.Generic;

namespace ConversorDadosXPTO.Models;

public partial class Cidadao
{
    public int Idcidadao { get; set; }

    public string? Nome { get; set; }

    public string? Cpf { get; set; }

    public virtual ICollection<Dado> Dados { get; set; } = new List<Dado>();
}
