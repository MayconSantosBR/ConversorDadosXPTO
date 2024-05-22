using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversorDadosXPTO.Models
{
    public class CidadeDto
    {
        public CidadeDto (int id, string uf, string nome)
        {
            Id = id;
            Uf = uf;
            Nome = nome;
        }

        public int Id { get; set; }

        public string? Uf { get; set; }

        public string? Nome { get; set; }
    }
}
