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

                    foreach (var item in seguroDefeso)
                    {
                        int cityId = 0;
                        var city = await _ufCidadeContext.GetByIdAsync(item.CityCode);

                        if (city == null)
                        {
                            UfCidade ufCidade = new UfCidade
                            {
                                IdufCidade = item.CityCode,
                                Cidade = item.CityName,
                                Uf = item.StateAcronym
                            };

                            cityId = await _ufCidadeContext.AddAsync(ufCidade);
                        }
                        else
                        {
                            cityId = city.IdufCidade;
                        }

                        int personId = 0;
                        var person = await _cidadaoContext.FindByConditionAsync(x => x.Cpf == OnlyNumbers(item.Cpf));

                        if (person == null || person.Count() == 0)
                        {
                            Cidadao cidadao = new Cidadao
                            {
                                Nome = item.Person,
                                Cpf = OnlyNumbers(item.Cpf)
                            };

                            personId = await _cidadaoContext.AddAsync(cidadao);
                        }
                        else
                        {
                            personId = person.FirstOrDefault().Idcidadao;
                        }

                        var dado = new Dado
                        {
                            IdprogramaSocial = fileId,
                            Idcidadao = personId,
                            IdufCidade = cityId,
                            MesAno = item.Date,
                            Valor = item.InstallmentValue
                        };

                        await _dadoContext.AddAsync(dado);
                    }
                }
                else if (fileName.ToLower().Contains("garantiasafra"))
                {
                    var garantiaSafras = TransformCsvToObjects<GarantiaSafra>(sheet);

                    foreach (var item in garantiaSafras)
                    {
                        int cityId = 0;
                        var city = await _ufCidadeContext.GetByIdAsync(item.CityCode);

                        if (city == null)
                        {
                            UfCidade ufCidade = new UfCidade
                            {
                                IdufCidade = item.CityCode,
                                Cidade = item.CityName,
                                Uf = item.StateAcronym
                            };

                            cityId = await _ufCidadeContext.AddAsync(ufCidade);
                        }
                        else
                        {
                            cityId = city.IdufCidade;
                        }

                        int personId = 0;
                        var person = await _cidadaoContext.FindByConditionAsync(x => x.Cpf == item.Nis.ToString());

                        if (person == null || person.Count() == 0)
                        {
                            Cidadao cidadao = new Cidadao
                            {
                                Nome = item.Person,
                                Cpf = item.Nis.ToString()
                            };

                            personId = await _cidadaoContext.AddAsync(cidadao);
                        }
                        else
                        {
                            personId = person.FirstOrDefault().Idcidadao;
                        }

                        var dado = new Dado
                        {
                            IdprogramaSocial = fileId,
                            Idcidadao = personId,
                            IdufCidade = cityId,
                            MesAno = item.Date,
                            Valor = item.InstallmentValue
                        };

                        await _dadoContext.AddAsync(dado);
                    }
                }
                else
                {
                    throw new Exception("Arquivo não reconhecido");
                }
            }
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

        public static string OnlyNumbers(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return new string(input.Where(char.IsDigit).ToArray());
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
