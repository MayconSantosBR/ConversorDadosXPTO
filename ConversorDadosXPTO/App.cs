using ConversorDadosXPTO.Context;
using ConversorDadosXPTO.Models;
using ConversorDadosXPTO.Services;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConversorDadosXPTO
{
    public class App : IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IEntityService<Cidadao> _cidadaoContext;
        private readonly IEntityService<Dado> _dadoContext;
        private readonly IEntityService<ProgramaSocial> _programaSocialContext;
        private readonly IEntityService<UfCidade> _ufCidadeContext;

        public App(IServiceCollection services)
        {
            var builder = services.BuildServiceProvider();
            _configuration = builder.GetService<IConfiguration>() ?? throw new ArgumentNullException(nameof(_configuration));
            _cidadaoContext = builder.GetService<IEntityService<Cidadao>>() ?? throw new ArgumentNullException(nameof(_cidadaoContext));
            _dadoContext = builder.GetService<IEntityService<Dado>>() ?? throw new ArgumentNullException(nameof(_dadoContext));
            _programaSocialContext = builder.GetService<IEntityService<ProgramaSocial>>() ?? throw new ArgumentNullException(nameof(_programaSocialContext));
            _ufCidadeContext = builder.GetService<IEntityService<UfCidade>>() ?? throw new ArgumentNullException(nameof(_ufCidadeContext));
        }

        public async Task Start()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            string exeDirectory = Path.GetDirectoryName(exePath);
            string solutionDirectory = Directory.GetParent(exeDirectory).Parent.Parent.FullName;

            var sheets = Directory.GetFiles(solutionDirectory, "*.csv", SearchOption.AllDirectories);

            foreach (var sheet in sheets)
            {
                int fileId = 0;
                var fileName = Path.GetFileName(sheet);

                var file = await _programaSocialContext.FindByConditionAsync(x => x.Nome == fileName);

                if (file.Any())
                {
                    await Console.Out.WriteLineAsync($"Arquivo {fileName} já foi importado, iremos atualizar.");
                    fileId = file.FirstOrDefault().IdprogramaSocial;
                }
                else
                {
                    ProgramaSocial programaSocial = new ProgramaSocial
                    {
                        Nome = fileName
                    };

                    fileId = await _programaSocialContext.AddAsync(programaSocial);
                }

                if (fileName.ToLower().Contains("segurodefeso"))
                {
                    var seguroDefeso = TransformCsvToObjects<SeguroDefeso>(sheet);

                    await CreateCitiesAndPeople(seguroDefeso.ToList<ICommon>());

                    var dados = seguroDefeso.Select(seguroDefeso =>
                        new Dado()
                        {
                            IdprogramaSocial = fileId,
                            Idcidadao = long.Parse(seguroDefeso.Nis),
                            IdufCidade = seguroDefeso.CityCode,
                            MesAno = seguroDefeso.Date,
                            Valor = seguroDefeso.InstallmentValue,
                        }
                    );

                    await _dadoContext.AddBatchAsync(dados);

                    await Console.Out.WriteLineAsync("Dados inseridos!");
                }
                else if (fileName.ToLower().Contains("garantiasafra"))
                {
                    var garantiaSafras = TransformCsvToObjects<GarantiaSafra>(sheet);

                    await CreateCitiesAndPeople(garantiaSafras.ToList<ICommon>());

                    List<Dado> dados = garantiaSafras.Select(garantiaSafra =>
                        new Dado()
                        {
                            IdprogramaSocial = fileId,
                            Idcidadao = long.Parse(garantiaSafra.Nis),
                            IdufCidade = garantiaSafra.CityCode,
                            MesAno = garantiaSafra.Date,
                            Valor = garantiaSafra.InstallmentValue,
                        }
                    ).ToList();

                    await _dadoContext.AddBatchAsync(dados);

                    await Console.Out.WriteLineAsync("Dados inseridos!");
                }
                else
                {
                    throw new Exception("Arquivo não reconhecido");
                }
            }
        }

        private async Task CreateCitiesAndPeople(List<ICommon> information)
        {
            var cities = information.Select(c => new UfCidade() { IdufCidade = c.CityCode, Cidade = c.CityName, Uf = c.StateAcronym}).DistinctBy(c => c.IdufCidade).ToList();
            await _ufCidadeContext.AddBatchAsync(cities);
            await Console.Out.WriteLineAsync("Cidades criadas e atualizadas!");

            var people = information.Select(p => new Cidadao() { Nome = p.Person, Cpf = p.Nis }).DistinctBy(p => p.Cpf).ToList();
            await _cidadaoContext.AddBatchAsync(people);
            await Console.Out.WriteLineAsync("Pessoas criadas e atualizadas!");
        }

        public static List<T> TransformCsvToObjects<T>(string csvFilePath)
        {
            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<T>().ToList();
                return records;
            }
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
