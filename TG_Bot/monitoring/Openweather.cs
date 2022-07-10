using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TG_Bot
{
    public partial class Openweather
    {
        public int Id { get; set; }
        public DateTime? DateTime { get; set; }

        [Column("temp")]
        public float? Temperature { get; set; }

        [Column("feels_like")]
        public float? TemperatureFeelsLike { get; set; }
        public float? Pressure { get; set; }
        public float? Humidity { get; set; }
        public float? WindSpeed { get; set; }
        public float? WindDeg { get; set; }
        public float? WindGust { get; set; }
        public float? CloudsAll { get; set; }
        public DateTime? Dt { get; set; }
        public DateTime? Sunrise { get; set; }  
        public DateTime? Sunset { get; set; }
        public int? SunriseText { get; set; }
        public int? SunsetText { get; set; }
        public string WeatherMain { get; set; }
        public string WeatherDescription { get; set; }
        public int WeatherId { get; set; }
    }
}
