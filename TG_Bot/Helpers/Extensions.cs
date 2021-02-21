using System;
using System.Collections.Generic;
using System.Text;
using TG_Bot.BusinessLayer.CCUModels;
using TG_Bot.monitoring;

namespace TG_Bot.Helpers
{
    public static class Extensions
    {
        public static string ToFormatted(this bool state)
        {
            return state ? "Вкл." : "Выкл.";
        }

        public static string ToFormatted(this int state)
        {
            return state == 1 ? "Вкл." : "Выкл.";
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
