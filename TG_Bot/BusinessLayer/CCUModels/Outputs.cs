namespace TG_Bot.BusinessLayer.CCUModels
{
    public class Outputs
    {
        /// <summary>
        /// Нагрев конвектеров
        /// </summary>
        public Output Relay1 = new Output() { Index = 0, Name = "Relay1" };

        /// <summary>
        /// Свободное реле
        /// </summary>
        public Output Relay2 = new Output() { Index = 1, Name = "Relay2" };

        /// <summary>
        /// Включение бойлера
        /// </summary>
        public Output Output1 = new Output() { Index = 2, Name = "Output1" };

        /// <summary>
        /// Тёплые полы
        /// </summary>
        public Output Output2 = new Output() { Index = 3, Name = "Output2" };

        /// <summary>
        /// Спальня 4
        /// </summary>
        public readonly Output Output3 = new Output() { Index = 4, Name = "Output3" };

        /// <summary>
        /// Кухня, полы
        /// </summary>
        public Output Output4 = new Output() { Index = 5, Name = "Output4" };

        /// <summary>
        /// Свободный выход
        /// </summary>
        public Output Output5 = new Output() { Index = 6, Name = "Output5" };

    }

    public class Output
    {
        public string Name { get; set; }

        public int Index { get; set; }

        public int Number => this.Index + 1;

        public int State { get; set; }

    }
}