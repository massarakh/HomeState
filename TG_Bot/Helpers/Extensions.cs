using System;
using System.Collections.Generic;
using System.Text;
using TG_Bot.monitoring;

namespace TG_Bot.Helpers
{
    public static class Extensions
    {
        //public static string ToStat(this Monitor state)
        //{
        //    return $"Время:               {state.Timestamp?.ToString("H':'mm':'ss d MMM yyyy")}\n" +
        //           $"Фаза 1:              {state.Phase1} А\n" +
        //           $"Фаза 2:              {state.Phase2} A\n" +
        //           $"Фаза 3:              {state.Phase3} A\n" +
        //           $"Сумма фаз:      {state.PhaseSumm} A\n" +
        //           $"Бойлер:             {state.BoilerState}\n" +
        //           $"Тёплые полы: {state.HeatFloor}\n" +
        //           $"Батареи:           {state.HeatBatteries}\n" +
        //           $"Гостиная (t°):   {state.TemperatureLivingRoom} °С\n" +
        //           $"Гостиная (%):   {state.HumidityLivingRoom} %\n" +
        //           $"Спальня (t°):    {state.TemperatureBedroom} °С\n" +
        //           $"Спальня (%):    {state.HumidityBedroom} %\n" +
        //           $"Сарай (t°):         {state.TemperatureBarn} °С\n" +
        //           $"Улица (t°):         {state.TemperatureOutside} °С\n" +
        //           $"Энергия:            {state.Energy} кВт⋅ч";
        //}

        public static string ToFormatted(this bool state)
        {
            return state ? "Вкл." : "Выкл.";
        }
    }
}
