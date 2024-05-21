using ConversorDadosXPTO;
using ConversorDadosXPTO.Context;
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
services.AddDbContext<DadosXptoContext>(
   options => options.UseMySql(db, ServerVersion.AutoDetect(db))
       .EnableSensitiveDataLogging()
       .LogTo(Console.WriteLine)
       .EnableDetailedErrors()
       );

using var app = new App(services);
await app.Start();