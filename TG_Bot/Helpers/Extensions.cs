using System;
using System.Collections.Generic;
using System.Text;
using TG_Bot.monitoring;

namespace TG_Bot.Helpers
{
    public static class Extensions
    {
        public static string ToFormatted(this bool state)
        {
            return state ? "Вкл." : "Выкл.";
        }
    }
}
