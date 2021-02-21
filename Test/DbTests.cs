using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TG_Bot.Helpers;
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

        [Fact]
        public async Task Statistics()
        {
            await using var context = _provider.GetService<_4stasContext>();
            DateTime dtS = DateTime.Now.StartOfWeek(DayOfWeek.Saturday);
            DateTime dtE = DateTime.Now.StartOfWeek(DayOfWeek.Sunday).AddDays(1).AddSeconds(-1);

            try
            {
                var records = context.Monitor.AsQueryable().Where(_ => _.Timestamp >= dtS && _.Timestamp <= dtE).ToList();
                int Count = records.Count;
                var minTemp = records.Aggregate((currMin, x) => currMin.TemperatureOutside < x.TemperatureOutside ? currMin : x);
                var maxTemp = records.Aggregate((currMin, x) => currMin.TemperatureOutside > x.TemperatureOutside ? currMin : x);
                float? summ = 0;
                foreach (Monitor record in records)
                {
                    summ += record.PhaseSumm;
                }

                var power = (summ/Count) * 220;
            }
            catch (Exception ex)
            {
            }
            //var records = context.Monitor.Select(_ => _.Timestamp > dtS && _.Timestamp <= dtE);
        }
    }
}
