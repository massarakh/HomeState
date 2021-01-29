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

        //public static int[] ToArray(this Outputs outputs)
        //{
        //    return new[]
        //    {
        //        outputs.Relay1,
        //        outputs.Relay2,
        //        outputs.Output1,
        //        outputs.Output2,
        //        outputs.Output3,
        //        outputs.Output4,
        //        outputs.Output5
        //    };
        //}
    }
}
