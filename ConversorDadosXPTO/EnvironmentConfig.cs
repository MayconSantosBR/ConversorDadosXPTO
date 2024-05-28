using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversorDadosXPTO
{
    public static class EnvironmentConfig
    {
        public static class MySql
        {
            public const string ConnectionString = "server=localhost;port=3306;database=dados_xpto;user=admin;password=admin";
        }
    }
}
