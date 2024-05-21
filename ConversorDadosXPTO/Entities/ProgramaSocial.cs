using System;
using System.Collections.Generic;

namespace ConversorDadosXPTO.Models;

public partial class ProgramaSocial
{
    public int IdprogramaSocial { get; set; }

    public string? Nome { get; set; }

    public virtual ICollection<Dado> Dados { get; set; } = new List<Dado>();
}
