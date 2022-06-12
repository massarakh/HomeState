namespace TG_Bot.BusinessLayer.Concrete
{
    public class Weather
    {
        public string Timestamp { get; set; }
        public string Date { get; set; }
        public float? Temperature { get; set; }
        public float? TemperatureFeelsLike { get; set; }
        public float? Pressure { get; internal set; }
        public float? Humidity { get; internal set; }
        public float? WindSpeed { get; internal set; }
        public string WindDirection { get; internal set; }
        public float? WindGust { get; internal set; }
        public string Sunrise { get; internal set; }
        public string Sunset { get; internal set; }
        public string WeatherMain { get; internal set; }
        public string WeatherDescription { get; internal set; }
    }
}