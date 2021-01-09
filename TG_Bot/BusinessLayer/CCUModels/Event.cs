namespace TG_Bot.BusinessLayer.CCUModels
{
    /// <summary>
    /// Событие
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Идентификатор события. Подтверждается командой AckEvents.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Тип события
        /// </summary>
        public string Type { get; set; }
    }

    /// <summary>
    /// Дополнительные параметры для событий InputActive и InputPassive:
    /// </summary>
    public class InputEvent : Event
    {
        /// <summary>
        /// Номер входа
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Номера привязанных разделов. Отсутствует - не поддерживается.
        /// </summary>
        public int[] Partitions { get; set; }
    }

    /// <summary>
    /// Дополнительные параметры для событий Arm, Disarm, Protect:
    /// </summary>
    public class ArmEvent : Event
    {
        /// <summary>
        /// Номер раздела. Отсутствует не поддерживается
        /// </summary>
        public int? Partition { get; set; }

        /// <summary>
        /// Источник изменения режима охраны.
        /// </summary>
        public Source Source { get; set; }
    }

    /// <summary>
    /// Параметры источника изменения режима охраны:
    /// </summary>
    public class Source
    {
        /// <summary>
        /// Тип источника изменения режима охраны.
        /// </summary>
        public string Type { get; set; }
    }

    /// <summary>
    /// Дополнительные параметры для источника изменения режима охраны TouchMemory
    /// </summary>
    public class SourceTouchMemory : Source
    {
        /// <summary>
        /// Номер ключа. Отсутствует - обязателен KeyName.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Имя ключа. Отсутствует - обязателен Key.
        /// </summary>
        public string KeyName { get; set; }
    }

    /// <summary>
    /// Дополнительные параметры для источников изменения режима охраны DTMF, SMS, CSD, Call
    /// </summary>
    public class SourceGuard : Source
    {
        /// <summary>
        /// Номер телефона. Отсутствует - нет данных.
        /// </summary>
        public string Phone { get; set; }
    }

    /// <summary>
    /// Дополнительные параметры для события ProfileApplied
    /// </summary>
    public class EventProfileApplied : Event
    {
        /// <summary>
        /// Номер профиля.
        /// </summary>
        public int Number { get; set; }
    }

    /// <summary>
    /// Дополнительные параметры для источников изменения режима охраны uGuardNet и Shell
    /// </summary>
    public class UGuardNet : Source
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName { get; set; }
    }

    /// <summary>
    /// Дополнительный параметр для события ExtRuntimeError
    /// </summary>
    public class EventRuntimeError : Event
    {
        /// <summary>
        /// Код ошибки
        /// </summary>
        public int ErrorCode { get; set; }
    }
}