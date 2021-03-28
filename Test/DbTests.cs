using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TG_Bot.Helpers;
using TG_Bot.monitoring;
using Xunit;
using Xunit.Abstractions;
using static TG_Bot.Helpers.Additions;

namespace Test
{

    public class DbTests : IClassFixture<DbFixture>
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ServiceProvider _provider;

        public DbTests(DbFixture fixture, ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
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
            GetMinMaxTemp(context, StatType.Weekend);
        }

        [Fact]
        public async Task CalcHour()
        {
            DateTime start = DateTime.Now.Date;
            DateTime end = DateTime.Now.Date.AddDays(1).AddTicks(-1);
            // подсчёт среднего за час
            // SELECT AVG(D13) as average, DATE_FORMAT(date_time, '%Y-%m-%d %H') as hour_datetime
            // FROM monitor WHERE date_time > '2020-12-13 16:00:00' AND date_time < '2020-12-13 17:00:00'
            // GROUP BY DATE_FORMAT(date_time, '%Y-%m-%d %H')
            // перемножение результата на стоимость киловатта в зависимости от времени дня
            // вывод почасовой статистики
            await using (var context = _provider.GetService<_4stasContext>())
            {
                var t = context.Monitor.AsQueryable()
                    .Where(rec =>
                        rec.Timestamp > new DateTime(2020, 12, 12, 0, 0, 0) &&
                        rec.Timestamp < new DateTime(2020, 12, 13, 0, 0, 0) && rec.Timestamp != null)
                    .AsEnumerable()
                    .GroupBy(rec => new { hour = rec.Timestamp?.Hour })
                    .Select(av => new { Average = av.Average(a => a.Energy), Hour = av.Key.hour });

                foreach (var v in t)
                {
                    _testOutputHelper.WriteLine($"Hour - {v.Hour}, Average - {v.Average}");
                }
                //_testOutputHelper.WriteLine(t.ToString());
            }
        }

        static DateTime RoundToNearestInterval(DateTime dt, TimeSpan d)
        {
            int f = 0;
            double m = (double)(dt.Ticks % d.Ticks) / d.Ticks;
            if (m >= 0.5)
                f = 1;
            return new DateTime(((dt.Ticks / d.Ticks) + f) * d.Ticks);
        }

        private string GetMinMaxTemp(_4stasContext context, StatType type)
        {
            DateTime start;
            DateTime end;
            switch (type)
            {
                case StatType.Day:
                    start = DateTime.Now.Date;
                    end = DateTime.Now.Date.AddDays(1).AddTicks(-1);
                    break;

                case StatType.Weekend:
                    start = DateTime.Now.StartOfWeek(DayOfWeek.Saturday);
                    end = DateTime.Now.StartOfWeek(DayOfWeek.Sunday).AddDays(1).AddTicks(-1);
                    break;

                case StatType.Week:
                    start = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
                    end = DateTime.Now.StartOfWeek(DayOfWeek.Sunday).AddDays(1).AddTicks(-1);
                    break;

                case StatType.Month:
                    start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    end = start.AddMonths(1).AddTicks(-1);
                    break;

                //case StatType.Season:
                //    start = DateTime.Now.StartOfSeason();
                //    end = start.AddMonths(3).AddTicks(-1);
                //    break;

                case StatType.Year:
                    start = new DateTime(DateTime.Now.Year, 1, 1);
                    end = start.AddYears(1).AddTicks(-1);
                    break;

                default:
                    return string.Empty;
            }
            var records = context.Monitor.AsQueryable().Where(_ => _.Timestamp >= start && _.Timestamp <= end).ToList();
            //var minTemp = records.Aggregate((currMin, x) => currMin.TemperatureOutside < x.TemperatureOutside ? currMin : x);
            //var maxTemp = records.Aggregate((currMin, x) => currMin.TemperatureOutside > x.TemperatureOutside ? currMin : x);

            //var a = records.Aggregate(
            // new
            // {
            //     minTemperature = (float?)float.MaxValue,
            //     maxTemperature = (float?)float.MinValue
            // },
            // (accumulator, o) => new
            // {
            //     minTemperature = o.TemperatureOutside < accumulator.minTemperature ? o.TemperatureOutside : accumulator.minTemperature, //Math.Min(o.TemperatureOutside, accumulator.minTemperature),
            //     maxTemperature = o.TemperatureOutside > accumulator.maxTemperature ? o.TemperatureOutside : accumulator.maxTemperature
            // });

            var sortedRecords = records.OrderByDescending(x => x.TemperatureOutside).ToList();
            var maxTemp = sortedRecords.First();
            var minTemp = sortedRecords.Last();

            return string.Empty;
        }


    }
}
