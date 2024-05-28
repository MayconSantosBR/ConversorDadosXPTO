using System;
using System.Collections.Generic;

namespace ConversorDadosXPTO.Models;

public partial class Dado
{
    public int Iddados { get; set; }

    public int IdufCidade { get; set; }

    public long Idcidadao { get; set; }

    public int IdprogramaSocial { get; set; }

    public string? MesAno { get; set; }

    public double? Valor { get; set; }

    public virtual Cidadao IdcidadaoNavigation { get; set; } = null!;

    public virtual ProgramaSocial IdprogramaSocialNavigation { get; set; } = null!;

    public virtual UfCidade IdufCidadeNavigation { get; set; } = null!;
}
