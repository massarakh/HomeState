using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NLog;
using TG_Bot.BusinessLayer.Abstract;
using TG_Bot.DAL;
using TG_Bot.Helpers;
using TG_Bot.monitoring;
using static TG_Bot.Helpers.Additions;

namespace TG_Bot.BusinessLayer.Concrete
{
    public class StateService : IStateService
    {
        private readonly IStateRepository _repository;
        private readonly IConfiguration _configuration;
        private decimal _priceDay;
        private decimal _priceNight;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public StateService(IStateRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
            (_priceDay, _priceNight) = GetPrices();
        }

        private (decimal _priceDay, decimal _priceNight) GetPrices()
        {
            IEnumerable<IConfigurationSection> sections = _configuration.GetSection("ElectricityPrices").GetChildren();
            var configurationSections = sections as IConfigurationSection[] ?? sections.ToArray();
            var day = Convert.ToDecimal(configurationSections.First(_ => _.Key == "Day").Value);
            var night = Convert.ToDecimal(configurationSections.First(_ => _.Key == "Night").Value);
            if (day == 0 || night == 0)
            {
                _logger.Warn($"Не найдены тарифы на электричество");
            }
            return (day, night);
        }

        /// <inheritdoc />
        public async Task<string> LastState()
        {
            var state = await _repository.GetState();
            //return $"<pre>" +
            //       $"</pre>";

            return $"<pre>" +
                   $"Время:       {state.Timestamp}\n" +
                   $"Дата:        {state.Date}\n" +
                   $"Фаза 1:      {state.Electricity.Phase1} А\n" +
                   $"Фаза 2:      {state.Electricity.Phase2} A\n" +
                   $"Фаза 3:      {state.Electricity.Phase3} A\n" +
                   $"Сумма фаз:   {state.Electricity.PhaseSumm} A\n" +
                   $"Энергия:     {state.Energy} кВт⋅ч\n" +
                   $"====================\n" +
                   $"Бойлер:      {state.Boiler.ToFormatted()}\n" +
                   $"Тёплые полы: {state.Heat.Floor.ToFormatted()}\n" +
                   $"Батареи:     {state.Heat.Batteries.ToFormatted()}\n" +
                   $"====================\n" +
                   $"Гостиная:    {state.Temperature.LivingRoom} °С | {state.Humidity.LivingRoom} %\n" +
                   $"Спальня №2:  {state.Temperature.Bedroom} °С | {state.Humidity.Bedroom} %\n" +
                   $"Спальня №4:  {state.Temperature.BedroomYouth} °С\n" +
                   $"Улица:       {state.Temperature.Outside} °С\n" +
                   $"Сарай:       {state.Temperature.Barn} °С</pre>";
        }

        /// <inheritdoc />
        public async Task<string> Electricity()
        {
            var state = await _repository.GetState();
            return $"<pre>" +
                   $"Время:    {state.Timestamp}\n" +
                   $"Дата:     {state.Date}\n" +
                   $"====================\n" +
                   $"Фаза 1:   {state.Electricity.Phase1} А\n" +
                   $"Фаза 2:   {state.Electricity.Phase2} A\n" +
                   $"Фаза 3:   {state.Electricity.Phase3} A\n" +
                   $"====================\n" +
                   $"Сумма фаз:{state.Electricity.PhaseSumm} A\n" +
                   $"Энергия:  {state.Energy}кВт⋅ч</pre>\n";
        }

        /// <inheritdoc />
        public async Task<string> Temperature()
        {
            var state = await _repository.GetState();
            return $"<pre>" +
                   $"Время:     {state.Timestamp}\n" +
                   $"Дата:      {state.Date}\n" +
                   $"====================\n" +
                   $"Гостиная:  {state.Temperature.LivingRoom} °С | {state.Humidity.LivingRoom} %\n" +
                   $"Спальня №2:{state.Temperature.Bedroom} °С | {state.Humidity.Bedroom} %\n" +
                   $"Спальня №4:{state.Temperature.BedroomYouth} °С\n" +
                   $"Улица:     {state.Temperature.Outside} °С\n" +
                   $"Сарай:     {state.Temperature.Barn} °С</pre>";
        }

        /// <inheritdoc />
        public async Task<string> Heating()
        {
            var state = await _repository.GetState();
            return $"<pre>" +
                   $"Бойлер:      {state.Boiler.ToFormatted()}\n" +
                   $"Тёплые полы: {state.Heat.Floor.ToFormatted()}\n" +
                   $"Батареи:     {state.Heat.Batteries.ToFormatted()}</pre>\n";
        }

        public async Task<string> GetStatistics(StatType type)
        {
            DateTime start;
            DateTime end;
            string result;
            switch (type)
            {
                case StatType.Day:
                    start = DateTime.Now.Date;
                    end = DateTime.Now.Date.AddDays(1).AddTicks(-1);

                    //start = DateTime.Now.Date.AddDays(-1);
                    //end = start.AddDays(1).AddTicks(-1);

                    result = "<pre>Сегодня (Мин/Макс): " +
                             "\n{0}°С / {1}°С</pre>";

                    break;

                case StatType.Weekend:
                    start = DateTime.Now.StartOfWeek(DayOfWeek.Saturday);
                    end = DateTime.Now.StartOfWeek(DayOfWeek.Sunday).AddDays(1).AddTicks(-1);
                    result = "<pre>Выходные " + start.Date.ToString("d'.'MM") + "-" + end.Date.ToString("d'.'MM") + " (Мин/Макс): " +
                             "\n{0}°С / {1}°С </pre>";
                    break;

                case StatType.Week:
                    start = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
                    end = start.AddDays(7).AddTicks(-1);
                    result = "<pre>Неделя " + start.Date.ToString("d'.'MM") + "-" + end.Date.ToString("d'.'MM") + " (Мин/Макс): \n{0}°С / {1}°С </pre>";
                    break;

                case StatType.Month:
                    start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    end = start.AddMonths(1).AddTicks(-1);
                    result = "<pre>" + start.ToString("MMMM", CultureInfo.GetCultureInfo("ru-RU")) + " (Мин/Макс): \n" +
                             "{0}°С\t{2}\n" +
                             "{1}°С \t{3}</pre>";
                    break;

                case StatType.Season:
                    start = DateTime.Now.StartOfSeason(out var season);
                    end = start.AddMonths(3).AddTicks(-1);
                    result = "<pre>" + season + " (Мин/Макс): \n" +
                             "{0}°С\t{2}\n" +
                             "{1}°С  \t{3}</pre>";
                    break;

                case StatType.Year:
                    start = new DateTime(DateTime.Now.Year, 1, 1);
                    end = start.AddYears(1).AddTicks(-1);
                    result = "<pre>Год " + start.Year + " (Мин/Макс): \n" +
                             "{0}°С\t{2}\n" +
                             "{1}°С  \t{3}</pre>";
                    break;

                default:
                    return string.Empty;
            }
            var records = await _repository.Query().Where(_ => _.Timestamp >= start && _.Timestamp <= end).ToListAsync();
            if (!records.Any())
            {
                return "Невозможно получить результаты, нет записей";
            }

            var sortedRecords = records.OrderByDescending(x => x.TemperatureOutside).ToList();
            var maxTemp = sortedRecords.First();
            var minTemp = sortedRecords.Last();

            string retValue = string.Format(result, minTemp.TemperatureOutside,
                maxTemp.TemperatureOutside,
                minTemp.Timestamp?.ToString("dd'.'MM H':'mm"),
                maxTemp.Timestamp?.ToString("dd'.'MM H':'mm"));

            List<ElectricityValues> list;
            StringBuilder sb;
            var electricity = "<pre>" +
                                 "\nЭлектричество:";
            //Вычисление электричества
            switch (type)
            {
                case StatType.Day:
                    try
                    {
                        start = DateTime.Now.AddHours(-24).AddTicks(-1);
                        end = DateTime.Now;
                        list = await GetElectricityValues(start, end);

                        sb = new StringBuilder();
                        sb.Append($"\nДень | Час | кВт⋅ч | ₽");
                        foreach (var rec in list)
                        {
                            var dt = DateTime.Parse(rec.Date).ToString("dd'.'MM");
                            var hour = rec.Hour?.ToString().Length == 1
                                ? "0" + rec.Hour.Value
                                : rec.Hour?.ToString();
                            sb.Append("\n" + dt + "| " + hour + "  | " + rec.Average.ToString("0.00") + " |" + rec.Price.ToString("0.00"));
                        }

                        electricity += sb.ToString();

                        //List<DayValue> tmp = list.FindAll(_ => DateTime.Parse(_.Date) == DateTime.Today)
                        //    .GroupBy(r => new { dt = r.Date })
                        //    .Select(rc => new DayValue
                        //    {
                        //        Date = rc.Key.dt,
                        //        AverageDay = rc.Sum(_ => _.Average),
                        //        Summ = rc.Sum(_ => _.Price)
                        //    }).ToList();

                        //sb.Append("\nВсего за день:");
                        //sb.Append($"\nкВт⋅ч | ₽");
                        //var d = DateTime.Parse().ToString("dd'.'MM");
                        //sb.Append("\n" + d + " | " + last.AverageDay.ToString("0.00") + " |" + last.Summ.ToString("0.00"));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Ошибка вычисления показаний электричества - {ex.Message}");
                        return "Ошибка вычисления показаний электричества";
                    }


                    break;

                case StatType.Weekend:
                    try
                    {
                        sb = new StringBuilder();
                        sb.Append($"\nДень  | кВт⋅ч | ₽");

                        //прошлые выходные
                        start = DateTime.Now.AddDays(-7).StartOfWeek(DayOfWeek.Saturday);
                        end = DateTime.Now.AddDays(-7).StartOfWeek(DayOfWeek.Sunday).AddDays(1).AddTicks(-1);
                        list = await GetElectricityValues(start, end);

                        var daySum = list.GroupBy(r => new { dt = r.Date })
                            .Select(rc => new
                            {
                                Date = rc.Key.dt,
                                AverageDay = rc.Sum(_ => _.Average),
                                Summ = rc.Sum(_ => _.Price)
                            });

                        foreach (var v in daySum)
                        {
                            var dt = DateTime.Parse(v.Date).ToString("dd'.'MM");
                            sb.Append("\n" + dt + " | " + v.AverageDay.ToString("0.00") + " |" + v.Summ.ToString("0.00"));
                        }

                        //последние выходные
                        start = DateTime.Now.StartOfWeek(DayOfWeek.Saturday);
                        end = DateTime.Now.StartOfWeek(DayOfWeek.Sunday).AddDays(1).AddTicks(-1);
                        list = await GetElectricityValues(start, end);

                        daySum = list.GroupBy(r => new { dt = r.Date })
                             .Select(rc => new
                             {
                                 Date = rc.Key.dt,
                                 AverageDay = rc.Sum(_ => _.Average),
                                 Summ = rc.Sum(_ => _.Price)
                             });

                        foreach (var v in daySum)
                        {
                            var dt = DateTime.Parse(v.Date).ToString("dd'.'MM");
                            sb.Append("\n" + dt + " | " + v.AverageDay.ToString("0.00") + " |" + v.Summ.ToString("0.00"));
                        }
                        electricity += sb.ToString();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Ошибка вычисления показаний электричества - {ex.Message}");
                        return "Ошибка вычисления показаний электричества";
                    }

                    break;

                case StatType.Month:

                    break;
            }

            return retValue + electricity + "</pre>";
        }

        private async Task<List<ElectricityValues>> GetElectricityValues(DateTime start, DateTime end)
        {
            var records = await _repository.Query().Where(_ => _.Timestamp >= start
                                                                && _.Timestamp <= end).ToListAsync();
            var list = records
                .Where(rec => rec.Timestamp != null)
                .AsEnumerable()
                .GroupBy(rec => new { hour = rec.Timestamp?.Hour, date = rec.Timestamp?.ToShortDateString() })
                .Select(av => new
                {
                    Average = Convert.ToDecimal(av.Average(a => a.Energy)),
                    Hour = av.Key.hour,
                    Date = av.Key.date
                })
                .Select(av => new ElectricityValues
                {
                    Average = av.Average,//.ToString("0.00"),
                    Hour = av.Hour,
                    Date = av.Date,
                    Price = (av.Hour.Value > 7 && av.Hour.Value < 23
                        ? av.Average * _priceDay
                        : av.Average * _priceNight)//.ToString("0.00")
                });
            return list.ToList();
        }

        public class ElectricityValues
        {
            public decimal Average;
            public int? Hour;
            public string Date;
            public decimal Price;
        }

        public class DayValue
        {
            public string Date;
            public decimal AverageDay;
            public decimal Summ;
        }

    }
}
