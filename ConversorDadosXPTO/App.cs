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
                var fileName = Path.GetFileName(sheet);

                ProgramaSocial programaSocial = new ProgramaSocial
                {
                    Nome = fileName
                };

                var fileId = await _programaSocialContext.AddAsync(programaSocial);

                if (fileName.ToLower().Contains("segurodefeso"))
                {
                    var seguroDefeso = TransformCsvToObjects<SeguroDefeso>(sheet);

                    await CreateCitiesAndPeople(seguroDefeso.ToList<ICommon>());

                    List<Dado> dados = [];

                    await Parallel.ForEachAsync(seguroDefeso, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, async (seguro, token) =>
                    {
                        var city = await _ufCidadeContext.FindByConditionAsync(x => x.IdufCidade == seguro.CityCode);
                        var person = await _cidadaoContext.FindByConditionAsync(x => x.Cpf == seguro.Nis.ToString());

                        var dado = new Dado
                        {
                            IdprogramaSocial = fileId,
                            Idcidadao = person.FirstOrDefault().Idcidadao,
                            IdufCidade = city.FirstOrDefault().IdufCidade,
                            MesAno = seguro.Date,
                            Valor = seguro.InstallmentValue
                        };

                        dados.Add(dado);

                        await Console.Out.WriteLineAsync($"Registro para {seguro.Nis} adicionado a lista de importação.");
                    });

                    await _dadoContext.AddBatchAsync(dados);

                    await Console.Out.WriteLineAsync("Dados inseridos!");
                }
                else if (fileName.ToLower().Contains("garantiasafra"))
                {
                    var garantiaSafras = TransformCsvToObjects<GarantiaSafra>(sheet);

                    await CreateCitiesAndPeople(garantiaSafras.ToList<ICommon>());

                    List<Dado> dados = [];

                    await Parallel.ForEachAsync(garantiaSafras, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, async (safra, token) =>
                    {
                        var city = await _ufCidadeContext.FindByConditionAsync(x => x.IdufCidade == safra.CityCode);
                        var person = await _cidadaoContext.FindByConditionAsync(x => x.Cpf == safra.Nis.ToString());

                        var dado = new Dado
                        {
                            IdprogramaSocial = fileId,
                            Idcidadao = person.FirstOrDefault().Idcidadao,
                            IdufCidade = city.FirstOrDefault().IdufCidade,
                            MesAno = safra.Date,
                            Valor = safra.InstallmentValue
                        };

                        dados.Add(dado);

                        await Console.Out.WriteLineAsync($"Registro para {safra.Nis} adicionado a lista de importação.");
                    });

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
