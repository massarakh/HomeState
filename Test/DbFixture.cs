using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TG_Bot.monitoring;

namespace Test
{
    public class DbFixture
    {
        public DbFixture()
        {
            var path = Directory.GetParent(AppContext.BaseDirectory).FullName;
            string baseDir = path.Substring(0, path.LastIndexOf("Test", StringComparison.Ordinal));
            string appsettingsPath = Path.Combine(baseDir, "TG_Bot");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(appsettingsPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddDbContext<_4stasContext>(options =>
                    {
                        string connectionString = configuration.GetConnectionString("DefaultConnection");
                        ServerVersion version = ServerVersion.AutoDetect(connectionString);
                        options.UseMySql(connectionString, version);
                    },
                    ServiceLifetime.Transient);

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; private set; }
    }
}
