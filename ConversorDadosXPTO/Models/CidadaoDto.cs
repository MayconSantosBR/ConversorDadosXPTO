using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversorDadosXPTO.Models
{
    public class CidadaoDto
    {
        public CidadaoDto(int id, string name, string document)
        {
            Id = id;
            Nome = name;
            CpfOrNis = document;
        }

        public int Id { get; set; }

        public string? Nome { get; set; }

        public string? CpfOrNis { get; set; }
    }
}
