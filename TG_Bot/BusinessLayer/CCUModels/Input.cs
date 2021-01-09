namespace TG_Bot.BusinessLayer.CCUModels
{
    /// <summary>
    /// Состояние входа
    /// </summary>
    public class Input
    {
        /// <summary>
        /// Состояние входа. 1 - активен, 0 - пассивен.
        /// </summary>
        public int Active { get; set; }

        /// <summary>
        /// Напряжение входа. Целое число - значение в
        /// дискретах. Перевод в вольты по формуле:
        /// дискреты * 10 / 4095.
        ///  </summary>
        public int Voltage { get; set; }
    }
}