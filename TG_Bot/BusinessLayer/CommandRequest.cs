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
        /// Номер выхода
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Состояние, 1 - вкл, 0 - выкл
        /// </summary>
        public int State { get; set; }
    }
}