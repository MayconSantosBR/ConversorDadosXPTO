using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConversorDadosXPTO
{
    public class App : IDisposable
    {
        private readonly IConfiguration _configuration;

        public App(IServiceCollection services)
        {
            var builder = services.BuildServiceProvider();
            _configuration = builder.GetService<IConfiguration>() ?? throw new ArgumentNullException(nameof(_configuration));
        }

        public async Task Start()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            string exeDirectory = Path.GetDirectoryName(exePath);
            string solutionDirectory = Directory.GetParent(exeDirectory).Parent.Parent.FullName;

            var sheets = Directory.GetFiles(solutionDirectory, "*.xlsx", SearchOption.AllDirectories);

            foreach (var sheet in sheets)
            {
                Console.WriteLine(sheet);
            }
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
