using System;
using System.Collections.Generic;
using System.Text;
using TG_Bot.monitoring;

namespace TG_Bot.Helpers
{
    public static class Extensions
    {
        public static string ToStat(this Monitor state)
        {
            return $"Фаза 1: {state.Phase1} А\n" +
                   $"Фаза 2: {state.Phase2} A";
        }
    }
}
