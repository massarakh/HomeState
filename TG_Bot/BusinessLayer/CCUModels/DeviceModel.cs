using System;

namespace TG_Bot.BusinessLayer.CCUModels
{
    public class DeviceModel
    {
        public string DeviceType { get; set; }
        public string DeviceMod { get; set; }
        public string ExtBoard { get; set; }
        public string InputsCount { get; set; }
        public string PartitionsCount { get; set; }
        public string HwVer { get; set; }
        public string FwVer { get; set; }
        public string BootVer { get; set; }
        public DateTime FwBuildDate { get; set; }
        public string CountryCode { get; set; }
        public string Serial { get; set; }
        public string IMEI { get; set; }
        public int uGuardVerCode { get; set; }
    }
}