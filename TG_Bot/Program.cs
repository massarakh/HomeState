using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StructureMap;
using TG_Bot.BusinessLayer;
using TG_Bot.DAL;
using TG_Bot.monitoring;

namespace TG_Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    string envAppSettings = $"appsettings.json".ToLower();
                    //string envAppSettings = $"appsettings.{context.HostingEnvironment.EnvironmentName}.json".ToLower();
                    IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                     .AddJsonFile(envAppSettings, optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables()
                    .Build();

                    services.AddSingleton(LoggerFactory.Create(builder =>
                    {
                        builder.AddConsole(_ => _.FormatterName = "Monitoring");
                    }));
                    services.AddLogging();
                    services.AddScoped<IStateRepository, StateRepository>();
                    services.AddScoped<ICamService, CamService>();
                    services.AddScoped<IStateService, StateService>();
                    services.AddScoped<IBotService, BotService>();
                    services.AddSingleton(configuration);
                    services.AddHostedService<BotService>();
                    var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                    services.AddDbContext<_4stasContext>(options =>
                    {
                        options.UseMySql(connectionString);
                    });
                    var container = new Container();
                    container.Configure(config =>
                    {
                        config.Scan(_ =>
                        {
                            _.AssemblyContainingType(typeof(Program));
                            _.WithDefaultConventions();
                        });
                        config.Populate(services);
                    });
                    // https://thinkrethink.net/2018/08/02/hostbuilder-ihost-ihostedserice-console-application/
                    // https://blog.bitscry.com/2017/05/30/appsettings-json-in-net-core-console-app/
                    // https://www.thecodebuzz.com/entity-framework-console-windows-form-application-netcore/
                    // https://wildermuth.com/2020/08/02/NET-Core-Console-Apps---A-Better-Way
                    // https://andrewlock.net/using-dependency-injection-in-a-net-core-console-application/

                });
    }
}
