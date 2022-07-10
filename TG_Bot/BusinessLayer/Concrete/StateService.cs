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
        private readonly IRestService _restService;
        private readonly IWeatherRepository _weatherRepository;
        private double _priceDay;
        private double _priceNight;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public StateService(IStateRepository repository, IConfiguration configuration, IRestService restService, IWeatherRepository weatherRepository)
        {
            _repository = repository;
            _configuration = configuration;
            _restService = restService;
            _weatherRepository = weatherRepository;
            (_priceDay, _priceNight) = GetPrices();
        }

        private (double _priceDay, double _priceNight) GetPrices()
        {
            IEnumerable<IConfigurationSection> sections = _configuration.GetSection("ElectricityPrices").GetChildren();
            var configurationSections = sections as IConfigurationSection[] ?? sections.ToArray();
            var day = Convert.ToDouble(configurationSections.First(_ => _.Key == "Day").Value);
            var night = Convert.ToDouble(configurationSections.First(_ => _.Key == "Night").Value);
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
            var controllerState = _restService.CheckConnectivity();

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
                   $"Полы (с/у):  {state.Heat.Floor.ToFormatted()}\n" +
                   $"Батареи:     {state.Heat.Batteries.ToFormatted()}\n" +
                   $"Спальня №4:  {state.BedroomYouth.ToFormatted()}\n" +
                   $"Кухня, полы: {state.WarmFloorKitchen.ToFormatted()}\n" +
                   $"Контроллер:  {controllerState.ToFormatted()}\n" +
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
                   $"Полы (с/у):  {state.Heat.Floor.ToFormatted()}\n" +
                   $"Батареи:     {state.Heat.Batteries.ToFormatted()}\n" +
                   $"Спальня №4:  {state.BedroomYouth.ToFormatted()}\n" +
                   $"Кухня, полы: {state.WarmFloorKitchen.ToFormatted()}</pre>\n";
        }

        public async Task<string> GetStatistics(StatType type)
        {
            DateTime start;
            DateTime end;
            string result;
            switch (type)
            {
                case StatType.Day:
                    start = DateTime.Now.AddHours(-24).AddTicks(-1);
                    end = DateTime.Now;
                    result = "<pre>За 24 часа (Мин/Макс): " +
                             "\n{0}°С / {1}°С</pre>";
                    break;

                case StatType.Weekend:
                    start = DateTime.Now.StartOfWeek(DayOfWeek.Saturday);
                    end = start.AddDays(2).AddTicks(-1);
                    result = "<pre>Выходные " + start.Date.ToString("d'.'MM") + "-" + end.Date.ToString("d'.'MM") + " (Мин/Макс): " +
                             "\n{0}°С / {1}°С </pre>";
                    break;

                case StatType.LastWeekend:
                    start = DateTime.Now.AddDays(-7).StartOfWeek(DayOfWeek.Saturday);
                    end = start.AddDays(2).AddTicks(-1);
                    result = "<pre>Выходные " + start.Date.ToString("d'.'MM") + "-" + end.Date.ToString("d'.'MM") + " (Мин/Макс): " +
                             "\n{0}°С / {1}°С </pre>";
                    break;

                case StatType.Week:
                    start = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
                    end = start.AddDays(7).AddTicks(-1);
                    result = "<pre>Неделя " + start.Date.ToString("d'.'MM") + "-" + end.Date.ToString("d'.'MM") + " (Мин/Макс): \n{0}°С / {1}°С </pre>";
                    break;

                case StatType.LastWeek:
                    start = DateTime.Now.AddDays(-7).StartOfWeek(DayOfWeek.Monday);
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

                case StatType.LastMonth:
                    start = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 1);
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
            var records = await _repository
                .Query()
                .Where(_ => _.Timestamp >= start && _.Timestamp <= end)
                .OrderByDescending(x => x.TemperatureOutside)
                .ToListAsync();
            if (!records.Any())
            {
                return "Невозможно получить результаты, нет записей";
            }

            //var sortedRecords = records.OrderByDescending(x => x.TemperatureOutside).ToList();
            var maxTemp = records.First();
            var minTemp = records.Last();

            string retValue = string.Format(result, minTemp.TemperatureOutside,
                maxTemp.TemperatureOutside,
                minTemp.Timestamp?.ToString("dd'.'MM H':'mm"),
                maxTemp.Timestamp?.ToString("dd'.'MM H':'mm"));

            List<ElectricityValues> list = new List<ElectricityValues>();
            StringBuilder sb;
            var electricity = "<pre>" +
                                 "\nЭлектричество:";

            //Вычисление электричества
            switch (type)
            {
                case StatType.Day:
                    try
                    {
                        list = GetElectricityValues(records);

                        sb = new StringBuilder();
                        sb.Append($"\n{"День",-6}|{"Час",-3}|{"кВт*ч",-5}|₽");
                        foreach (var rec in list)
                        {
                            var dt = rec.Date.ToString("dd'.'MM");
                            var hour = rec.Hour?.ToString().Length == 1
                                ? "0" + rec.Hour.Value
                                : rec.Hour?.ToString();
                            var average = rec.Average.ToString("0.00");
                            var price = rec.Price.ToString("0.00");
                            sb.Append($"\n{dt,-6}|{hour,-3}|{average,-5}|{price}");
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
                case StatType.LastWeekend:
                case StatType.Week:
                case StatType.LastWeek:
                case StatType.Month:
                case StatType.LastMonth:
                    try
                    {
                        sb = new StringBuilder();
                        sb.Append($"\n{"День",-6}|{"кВт*ч",-7}|₽");
                        list = GetElectricityValues(records);

                        var daySum = list.GroupBy(r => new { dt = r.Date })
                            .Select(rc => new
                            {
                                Date = rc.Key.dt,
                                AverageDay = rc.Sum(_ => _.Average).ToString("0.00"),
                                Summ = rc.Sum(_ => _.Price).ToString("0.00")
                            });
                        
                        foreach (var v in daySum)
                        {
                            var dt = v.Date.ToString("dd'.'MM");
                            sb.Append($"\n{dt,-6}|{v.AverageDay,-7}|{v.Summ}");

                            //if (v.Date.DayOfWeek == DayOfWeek.Saturday || v.Date.DayOfWeek == DayOfWeek.Sunday)
                            //{
                            //    sb.Append($"\n{dt,-6}|{v.AverageDay,-7}|{v.Summ}");
                            //}
                            //else
                            //{
                            //    sb.Append($"\n{dt,-6}|{v.AverageDay,-7}|{v.Summ}");
                            //}
                        }

                        electricity += sb.ToString();

                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Ошибка вычисления показаний электричества - {ex.Message}");
                        return "Ошибка вычисления показаний электричества";
                    }

                    break;

                case StatType.Season:
                    try
                    {
                        sb = new StringBuilder();
                        sb.Append($"\n{"Месяц",-7}|{"кВт*ч",-7}|₽");

                        //TODO надо написать подсчёт для месяцев
                        list = GetElectricityValues(records);

                        var monthSum = list.GroupBy(r => new { dt = r.Date.Month })
                            .Select(rc => new
                            {
                                Date = rc.Key.dt,
                                AverageDay = rc.Sum(_ => _.Average).ToString("0.00"),
                                Summ = rc.Sum(_ => _.Price).ToString("0.00")
                            });

                        foreach (var v in monthSum)
                        {
                            var dt = v.Date.ToString("dd'.'MM");
                            sb.Append($"\n{dt,-6}|{v.AverageDay,-7}|{v.Summ}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Ошибка вычисления показаний электричества - {ex.Message}");
                        return "Ошибка вычисления показаний электричества";
                    }
                    break;
            }

            if (list.Count != 0)
            {
                double totalKw = 0;
                double totalCost = 0;
                Array.ForEach(list.ToArray(), i =>
                {
                    totalKw += i.Average;
                    totalCost += i.Price;
                });
                electricity += $"\n\nВсего за период:";
                electricity += $"\n{totalKw:0.00} кВт*ч";
                electricity += $"\n{totalCost:0.00} ₽";
            }
            
            return retValue + electricity + "</pre>";
        }

        /// <inheritdoc />
        public async Task<string> GetWeather()
        {
            var weather = await _weatherRepository.GetLastWeather();
            return $"<pre>" +
                   $"Время:         {weather.Timestamp}\n" +
                   $"Дата:          {weather.Date}\n" +
                   $"====================\n" +
                   $"Температура:   {weather.Temperature} °С\n" +
                   $"Ощущается как: {weather.TemperatureFeelsLike} °С\n" +
                   $"Влажность:     {weather.Humidity} %\n" +
                   $"Давление:      {weather.Pressure} мм рт.ст.\n" +
                   $"Ветер:         {weather.WindSpeed} м/с ({weather.WindDirection})\n" +
                   $"Порывами до:   {weather.WindGust} м/с\n" +
                   $"Погода:        {weather.WeatherMain}\n" +
                   $"====================\n" +
                   $"Восход:        {weather.Sunrise}\n" +
                   $"Закат:         {weather.Sunset}" +
                   $"</pre>";
        }

        private List<ElectricityValues> GetElectricityValues(List<Monitor> records)
        {
            var list = records
                .Where(rec => rec.Timestamp != null)
                .AsEnumerable()
                .GroupBy(rec => new { hour = rec.Timestamp?.Hour, date = rec.Timestamp?.ToShortDateString() })
                .Select(av => new
                {
                    Average = Convert.ToDouble(av.Average(a => a.Energy)),
                    Hour = av.Key.hour,
                    Date = av.Key.date
                })
                .Select(av => new ElectricityValues
                {
                    Average = Math.Round(av.Average, 2, MidpointRounding.AwayFromZero),
                    Hour = av.Hour,
                    Date = Convert.ToDateTime(av.Date),
                    Price = Math.Round(av.Hour.Value > 7 && av.Hour.Value < 23
                        ? av.Average * _priceDay
                        : av.Average * _priceNight, 2, MidpointRounding.AwayFromZero),
                }).OrderBy(x => x.Date).ThenBy(x => x.Hour);
            return list.ToList();
        }

        private class ElectricityValues
        {
            public double Average;
            public int? Hour;
            public DateTime Date;
            public double Price;
        }

    }
}
