using Newtonsoft.Json;

namespace TG_Bot.BusinessLayer.CCUModels
{
    public class Outputs
    {
        /// <summary>
        /// Нагрев конвектеров
        /// </summary>
        public Output Relay1 = new Output() { Index = 0, Meaning = "Relay1", Name = "Нагрев конвекторов" };

        /// <summary>
        /// Свободное реле
        /// </summary>
        public Output Relay2 = new Output() { Index = 1, Meaning = "Relay2", Name = "Relay2" };

        /// <summary>
        /// Включение бойлера
        /// </summary>
        public Output Output1 = new Output() { Index = 2, Meaning = "Output1", Name = "Бойлер" };

        /// <summary>
        /// Тёплые полы
        /// </summary>
        public Output Output2 = new Output() { Index = 3, Meaning = "Output2", Name = "Тёплые полы" };

        /// <summary>
        /// Спальня 4
        /// </summary>
        public readonly Output Output3 = new Output() { Index = 4, Meaning = "Output3", Name = "Спальня молодёжи" };

        /// <summary>
        /// Кухня, полы
        /// </summary>
        public Output Output4 = new Output() { Index = 5, Meaning = "Output4", Name = "Кухня" };

        /// <summary>
        /// Свободный выход
        /// </summary>
        public Output Output5 = new Output() { Index = 6, Meaning = "Output5", Name = "Output5" };

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