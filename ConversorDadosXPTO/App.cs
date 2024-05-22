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

                    await Parallel.ForEachAsync(seguroDefeso, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, async (seguro, token) =>
                    {
                        var city = await _ufCidadeContext.GetByIdAsync(seguro.CityCode);
                        var person = await _cidadaoContext.FindByConditionAsync(x => x.Cpf == seguro.Nis.ToString());

                        var dado = new Dado
                        {
                            IdprogramaSocial = fileId,
                            Idcidadao = person.FirstOrDefault().Idcidadao,
                            IdufCidade = city.IdufCidade,
                            MesAno = seguro.Date,
                            Valor = seguro.InstallmentValue
                        };

                        await _dadoContext.AddAsync(dado);
                    });
                }
                else if (fileName.ToLower().Contains("garantiasafra"))
                {
                    var garantiaSafras = TransformCsvToObjects<GarantiaSafra>(sheet);

                    await CreateCitiesAndPeople(garantiaSafras.ToList<ICommon>());

                    await Parallel.ForEachAsync(garantiaSafras, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, async (safra, token) =>
                    {
                        var city = await _ufCidadeContext.GetByIdAsync(safra.CityCode);
                        var person = await _cidadaoContext.FindByConditionAsync(x => x.Cpf == safra.Nis.ToString());

                        var dado = new Dado
                        {
                            IdprogramaSocial = fileId,
                            Idcidadao = person.FirstOrDefault().Idcidadao,
                            IdufCidade = city.IdufCidade,
                            MesAno = safra.Date,
                            Valor = safra.InstallmentValue
                        };

                        await _dadoContext.AddAsync(dado);
                    });
                }
                else
                {
                    throw new Exception("Arquivo não reconhecido");
                }
            }
        }

        private async Task CreateCitiesAndPeople(List<ICommon> information)
        {
            var cities = information.Select(c => new CidadeDto(c.CityCode, c.StateAcronym, c.CityName)).DistinctBy(c => c.Id).ToList();

            await Parallel.ForEachAsync(cities, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, async (city, token) =>
            {
                var cityInDatabase = await _ufCidadeContext.GetByIdAsync(city.Id);

                if (cityInDatabase == null)
                {
                    UfCidade ufCidade = new UfCidade
                    {
                        IdufCidade = city.Id,
                        Cidade = city.Nome,
                        Uf = city.Uf
                    };

                    await _ufCidadeContext.AddAsync(ufCidade);
                }
            });

            var people = information.Select(p => new CidadaoDto(0, p.Person, p.Nis)).DistinctBy(p => p.CpfOrNis).ToList();

            await Parallel.ForEachAsync(people, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, async (person, token) =>
            {
                var personInDatabase = await _cidadaoContext.FindByConditionAsync(x => x.Cpf == person.CpfOrNis);

                if (personInDatabase == null || personInDatabase.Count() == 0)
                {
                    Cidadao cidadao = new Cidadao
                    {
                        Nome = person.Nome,
                        Cpf = person.CpfOrNis
                    };

                    await _cidadaoContext.AddAsync(cidadao);
                }
            });
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
