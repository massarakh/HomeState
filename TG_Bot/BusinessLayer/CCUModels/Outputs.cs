using Newtonsoft.Json;

namespace TG_Bot.BusinessLayer.CCUModels
{
    public static class Outputs
    {
        /// <summary>
        /// Нагрев конвектеров
        /// </summary>
        public static Output Relay1 = new Output() { Index = 0, Meaning = "Relay1", Name = "Нагрев конвекторов" };

        /// <summary>
        /// Свободное реле
        /// </summary>
        public static Output Relay2 = new Output() { Index = 1, Meaning = "Relay2", Name = "Relay2" };

        /// <summary>
        /// Включение бойлера
        /// </summary>
        public static Output Output1 = new Output() { Index = 2, Meaning = "Output1", Name = "Бойлер" };

        /// <summary>
        /// Тёплые полы
        /// </summary>
        public static Output Output2 = new Output() { Index = 3, Meaning = "Output2", Name = "Тёплые полы" };

        /// <summary>
        /// Спальня 4
        /// </summary>
        public static readonly Output Output3 = new Output() { Index = 4, Meaning = "Output3", Name = "Спальня молодёжи" };

        /// <summary>
        /// Кухня, полы
        /// </summary>
        public static Output Output4 = new Output() { Index = 5, Meaning = "Output4", Name = "Кухня" };

        /// <summary>
        /// Свободный выход
        /// </summary>
        public static Output Output5 = new Output() { Index = 6, Meaning = "Output5", Name = "Бассейн" };

    }

    [JsonObject]
    public class Output
    {
        [JsonIgnore]
        public string Name { get; set; }

        [JsonIgnore]
        public int Index { get; set; }

        [JsonProperty]
        public int Number => this.Index + 1;

        [JsonIgnore]
        public string Meaning { get; set; }
    }
}