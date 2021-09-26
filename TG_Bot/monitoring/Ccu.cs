using System;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TG_Bot.monitoring
{
    public partial class Ccu
    {
        public int Id { get; set; }
        public DateTime? DateTime { get; set; }
        public string ClientIp { get; set; }
        public float? In1 { get; set; }
        public float? In2 { get; set; }
        public float? In3 { get; set; }
        public float? In4 { get; set; }
        public float? In5 { get; set; }
        public float? In6 { get; set; }
        public float? In7 { get; set; }
        public float? In8 { get; set; }
        public float? R1 { get; set; }
        public float? R2 { get; set; }

        /// <summary>
        /// Бойлер
        /// </summary>
        [Column("O1")]
        public float? Boiler { get; set; }

        /// <summary>
        /// Тёплые полы c/y
        /// </summary>
        [Column("O2")]
        public float? WarmFloorsBath { get; set; }

        /// <summary>
        /// Спальня №4
        /// </summary>
        [Column("O3")]
        public float? BedroomYouth { get; set; }

        /// <summary>
        /// Спальня №4
        /// </summary>
        [Column("O4")]
        public float? WarmFloorKitchen { get; set; }

        public float? O5 { get; set; }
        public string Mode { get; set; }
        public float? Battery { get; set; }
        public string BattState { get; set; }
        public float Balance { get; set; }
        public float Temp { get; set; }
        public float DcPower { get; set; }
    }
}
