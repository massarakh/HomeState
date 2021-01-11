namespace TG_Bot.BusinessLayer.CCUModels
{
    /// <summary>
    /// Состояние батареи
    /// </summary>
    public class Battery
    {
        /// <summary>
        /// "Low2" / "Low1" / "OK" /
        /// "NotUsed" / "Disconnected"
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Состояние батареи.
        /// "Low2" - разряд до 2 уровня,
        /// "Low1" - разряд до 1 уровня,
        /// "OK" - норма,
        /// "NotUsed" - не использовалась,
        /// "Disconnected" - отключена.
        /// </summary>
        public int? Charge { get; set; }
    }
}