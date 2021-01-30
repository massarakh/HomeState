using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TG_Bot.BusinessLayer.CCUModels;

namespace TG_Bot.BusinessLayer
{
    /// <summary>
    /// Команда изменения состояния выхода
    /// </summary>
    public class CommandRequest
    {
        /// <summary>
        /// Команда, имя API-метода
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Выход
        /// </summary>
        [JsonIgnore]
        public Output Output { get; set; }

        /// <summary>
        /// Номер выхода
        /// </summary>
        [JsonProperty]
        public int? Number => Output?.Number;

        /// <summary>
        /// Состояние, 1 - вкл, 0 - выкл
        /// </summary>
        public int State { get; set; }
    }
}