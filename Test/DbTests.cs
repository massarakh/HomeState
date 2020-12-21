using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TG_Bot.monitoring;
using Xunit;

namespace Test
{

    public class DbTests : IClassFixture<DbFixture>
    {
        private readonly ServiceProvider _provider;

        public DbTests(DbFixture fixture)
        {
            _provider = fixture.ServiceProvider;
        }

        [Fact]
        public async Task GetState()
        {
            await using var context = _provider.GetService<_4stasContext>();
            var lastRecord = await context.Monitor
                .OrderByDescending(d => d.Timestamp)
                .FirstOrDefaultAsync();
            Assert.True(lastRecord != null);
        }
    }
}
