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
            return $"<pre>Время:           {state.Timestamp}\n" +
                   $"Фаза 1:          {state.Electricity.Phase1} А\n" +
                   $"Фаза 2:          {state.Electricity.Phase2} A\n" +
                   $"Фаза 3:          {state.Electricity.Phase3} A\n" +
                   $"Сумма фаз:       {state.Electricity.PhaseSumm} A\n" +
                   $"Бойлер, питание: {state.Boiler.ToFormatted()}\n" +
                   $"Тёплые полы:     {state.Heat.Floor.ToFormatted()}\n" +
                   $"Батареи:         {state.Heat.Batteries.ToFormatted()}\n" +
                   $"Гостиная (t°):   {state.Temperature.LivingRoom} °С\n" +
                   $"Гостиная (%):    {state.Humidity.LivingRoom} %\n" +
                   $"Спальня (t°):    {state.Temperature.Bedroom} °С\n" +
                   $"Спальня (%):     {state.Humidity.Bedroom} %\n" +
                   $"Сарай (t°):      {state.Temperature.Barn} °С\n" +
                   $"Улица (t°):      {state.Temperature.Outside} °С\n" +
                   $"Энергия:         {state.Energy} кВт⋅ч</pre>";
        }

        /// <inheritdoc />
        public async Task<string> Electricity()
        {
            var state = await _repository.GetState();
            return $"<pre>Время:     {state.Timestamp}\n" +
                   $"Фаза 1:    {state.Electricity.Phase1} А\n" +
                   $"Фаза 2:    {state.Electricity.Phase2} A\n" +
                   $"Фаза 3:    {state.Electricity.Phase3} A\n" +
                   $"Сумма фаз: {state.Electricity.PhaseSumm} A</pre>\n";
        }

        /// <inheritdoc />
        public async Task<string> Temperature()
        {
            var state = await _repository.GetState();
            return $"<pre>Время:         {state.Timestamp}\n" +
                   $"Гостиная (t°): {state.Temperature.LivingRoom} °С\n" +
                   $"Гостиная (%):  {state.Humidity.LivingRoom} %\n" +
                   $"Спальня (t°):  {state.Temperature.Bedroom} °С\n" +
                   $"Спальня (%):   {state.Humidity.Bedroom} %\n" +
                   $"Сарай (t°):    {state.Temperature.Barn} °С\n" +
                   $"Улица (t°):    {state.Temperature.Outside} °С</pre>\n";

        }

        /// <inheritdoc />
        public async Task<string> Heating()
        {
            var state = await _repository.GetState();
            return $"<pre>Бойлер:        {state.Boiler.ToFormatted()}\n" +
                   $"Тёплые полы:   {state.Heat.Floor.ToFormatted()}\n" +
                   $"Батареи:       {state.Heat.Batteries.ToFormatted()}</pre>\n";
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

            string electricity = string.Empty;
            //Вычисление электричества
            switch (type)
            {
                case StatType.Day:
                    electricity = "<pre>" +
                                  "\nЭлектричество:";

                    try
                    {
                        start = DateTime.Now.AddHours(-24).AddTicks(-1);
                        end = DateTime.Now;
                        records = await _repository.Query().Where(_ => _.Timestamp >= start
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
                            .Select(av => new
                            {
                                Average = av.Average.ToString("0.00"),
                                av.Hour,
                                av.Date,
                                Price = (av.Hour.Value > 7 && av.Hour.Value < 23
                                    ? av.Average * _priceDay
                                    : av.Average * _priceNight).ToString("0.00")
                            });

                        StringBuilder sb = new StringBuilder();
                        sb.Append($"\nДень | Час | кВт⋅ч | ₽");
                        foreach (var rec in list)
                        {
                            var dt = DateTime.Parse(rec.Date).ToString("d'.'MM");
                            var hour = rec.Hour?.ToString().Length == 1 
                                ? "0" + rec.Hour.Value 
                                : rec.Hour?.ToString();
                            sb.Append("\n" + dt + "| " + hour + "  | " + rec.Average + "  |" + rec.Price);
                        }

                        electricity += sb.ToString();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Ошибка вычисления показаний электричества - {ex.Message}");
                        return "Ошибка вычисления показаний электричества";
                    }
                    break;

                case StatType.Weekend:

                    break;

                case StatType.Month:

                    break;
            }

            return retValue + electricity+"</pre>";
        }

        //private bool BoilerHeat(Data data)
        //{
        //    if (data.Boiler && data.Electricity.Phase3)
        //}

        //[NotMapped]
        //public string HeatFloor
        //{
        //    get
        //    {
        //        int valHeat = Convert.ToInt32(Heat);
        //        if (valHeat == 3 || valHeat == 9)
        //        {
        //            //return new SingleEmoji(new UnicodeSequence("1F7E2"), "green circle", new[] { "green", "circle" }, 1).ToString();
        //            return "Вкл.";
        //        }
        //        //return new SingleEmoji(new UnicodeSequence("1F534"), "red circle", new[] { "red", "circle" }, 1).ToString();
        //        return "Выкл.";
        //    }
        //}

        //[NotMapped]
        //public string HeatBatteries
        //{
        //    get
        //    {
        //        int valHeat = Convert.ToInt32(Heat);
        //        if (valHeat == 6 || valHeat == 9)
        //        {
        //            return "Вкл.";
        //            //return new SingleEmoji(new UnicodeSequence("1F7E2"), "green circle", new[] { "green", "circle" }, 1).ToString();
        //        }
        //        //return new SingleEmoji(new UnicodeSequence("1F534"), "red circle", new[] { "red", "circle" }, 1).ToString();
        //        return "Выкл.";
        //    }
        //}

        //[NotMapped]
        //public string BoilerState => Convert.ToInt32(Boiler) == 0 ? "Выкл." : "Вкл.";
        ////public string BoilerState => Convert.ToInt32(Boiler) == 0 ?
        ////    new SingleEmoji(new UnicodeSequence("1F534"), "red circle", new[] { "red", "circle" }, 1).ToString()
        ////    : new SingleEmoji(new UnicodeSequence("1F7E2"), "green circle", new[] { "green", "circle" }, 1).ToString();

    }
}
