using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TG_Bot.monitoring
{
    public partial class Monitor
    {
        public int Id { get; set; }

        [Column("date_time")]
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// IP-адрес клиента
        /// </summary>
        public string ClientIp { get; set; }

        /// <summary>
        /// Фаза 1
        /// </summary>
        [Column("D1")]
        public float? Phase1 { get; set; }

        /// <summary>
        /// Фаза 2
        /// </summary>
        [Column("D2")]
        public float? Phase2 { get; set; }

        /// <summary>
        /// Фаза 3
        /// </summary>
        [Column("D3")]
        public float? Phase3 { get; set; }

        /// <summary>
        /// Температура в гостиной
        /// </summary>
        [Column("D4")]
        public float? TemperatureLivingRoom { get; set; }

        /// <summary>
        /// Влажность в гостиной
        /// </summary>
        [Column("D5")]
        public float? HumidityLivingRoom { get; set; }

        /// <summary>
        /// Температура на улице
        /// </summary>
        [Column("D6")]
        public float? TemperatureOutside { get; set; }

        /// <summary>
        /// Температура в сарае
        /// </summary>
        [Column("D7")]
        public float? TemperatureBarn { get; set; }

        /// <summary>
        /// Температура в спальне
        /// </summary>
        [Column("D8")]
        public float? TemperatureBedroom { get; set; }

        /// <summary>
        /// Влажность в спальне
        /// </summary>
        [Column("D9")]
        public float? HumidityBedroom { get; set; }

        /// <summary>
        /// Сумма всех фаз
        /// </summary>
        [Column("D10")]
        public float? PhaseSumm { get; set; }

        /// <summary>
        /// Нагрев элементов
        /// </summary>
        [Column("D11")]
        public float? Heat { get; set; }

        /// <summary>
        /// Бойлер
        /// </summary>
        [Column("D12")]
        public float? Boiler { get; set; }

        /// <summary>
        /// Потребляемая мощность в кВт*ч
        /// </summary>
        [Column("D13")]
        public float? Energy { get; set; }


        public float? D14 { get; set; }
    }
}
