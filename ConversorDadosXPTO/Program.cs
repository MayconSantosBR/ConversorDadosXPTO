using ConversorDadosXPTO;
using ConversorDadosXPTO.Context;
using ConversorDadosXPTO.Repositories;
using ConversorDadosXPTO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", true, true)
    .AddEnvironmentVariables()
    .Build();

var db = config.GetConnectionString("MySql");

var services = new ServiceCollection();
services.AddScoped<IConfiguration>(c => { return config; });
services.AddDbContextFactory<DadosXptoContext>(
   options => options.UseMySql(db, ServerVersion.AutoDetect(db))
       .LogTo(Console.WriteLine)
       .EnableDetailedErrors()
       );

services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
services.AddScoped(typeof(IEntityService<>), typeof(EntityService<>));

using var app = new App(services);
await app.Start();