using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TG_Bot.BusinessLayer.Concrete;
using TG_Bot.monitoring;

namespace TG_Bot.DAL
{
    public class WeatherRepository : IWeatherRepository
    {
        private readonly _4stasContext _context;

        public WeatherRepository(_4stasContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public IQueryable<Openweather> Query()
        {
            return _context.Weather.AsQueryable();
        }

        /// <inheritdoc />
        public async Task<Weather> GetLastWeather()
        {
            string[] caridnals = { "С", "ССВ", "СВ", "ВСВ", "В", "ВЮВ", "ЮВ", "ЮЮВ", "Ю", "ЮЮЗ", "ЮЗ", "ЗЮЗ", "З", "ЗСЗ", "СЗ", "ССЗ", "С" };

            Openweather currentWeather = await _context.Weather
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();
            
            return new Weather
            {
                Timestamp = currentWeather.DateTime?.ToString("H':'mm"),
                Date = currentWeather.DateTime?.ToString("d'.'MM'.'yy"),
                Temperature = currentWeather.Temperature,
                TemperatureFeelsLike = currentWeather.TemperatureFeelsLike,
                Pressure = currentWeather.Pressure,
                Humidity = currentWeather.Humidity,
                WindSpeed = currentWeather.WindSpeed,
                WindDirection = currentWeather.WindDeg != null
                    ? caridnals[(int)Math.Round((double)currentWeather.WindDeg * 10 % 3600 / 225)]
                    : "-",
                WindGust = currentWeather.WindGust,
                Sunrise = currentWeather.Sunrise?.ToString("H':'mm':'ss"),
                Sunset = currentWeather.Sunset?.ToString("H':'mm':'ss"),
                WeatherMain = currentWeather.WeatherMain,
                WeatherDescription = currentWeather.WeatherDescription,
            };
            
        }
    }
}