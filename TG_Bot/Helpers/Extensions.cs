using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using TG_Bot.BusinessLayer.CCUModels;
using TG_Bot.monitoring;

namespace TG_Bot.Helpers
{
    public static class Extensions
    {
        private static string enable = char.ConvertFromUtf32(0x2705);
        private static string disable = char.ConvertFromUtf32(0x1F6D1);

        public static string ToFormatted(this bool state)
        {
            return state ? $"{enable}" : $"{disable}";
            //return state ? "Вкл." : "Выкл.";
        }

        public static string ToFormatted(this int state)
        {
            return state == 1 ? $"{enable}" : $"{disable}";
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public static DateTime StartOfSeason(this DateTime dt, out string season)
        {
            int currMonth = dt.Month;
            season = string.Empty;
            DateTime startDate = DateTime.MinValue;
            if (currMonth == 12 || currMonth == 1 || currMonth == 2)
            {
                season = "Зима";
                //winter
                switch (currMonth)
                {
                    case 12:
                        startDate = new DateTime(DateTime.Now.Year, 12, 1);
                        break;

                    case 1:
                    case 2:
                        startDate = new DateTime(DateTime.Now.Year - 1, 12, 1);
                        break;
                }
            }
            else if (currMonth == 3 || currMonth == 4 || currMonth == 5)
            {
                //spring
                season = "Весна";
                startDate = new DateTime(DateTime.Now.Year, 3, 1);
            }
            else if (currMonth == 6 || currMonth == 7 || currMonth == 8)
            {
                //summer
                season = "Лето";
                startDate = new DateTime(DateTime.Now.Year, 6, 1);
            }
            else if (currMonth == 9 || currMonth == 10 || currMonth == 11)
            {
                //autumn
                season = "Осень";
                startDate = new DateTime(DateTime.Now.Year, 9, 1);
            }

            return startDate;
        }

        public static string GetCurrentSeason()
        {
            DateTime.Now.StartOfSeason(out var season);
            return season;
        }
    }

    public class Additions
    {
        public enum StatType
        {
            Day = 0,
            Weekend = 1,
            Week = 2,
            Month = 3,
            Season = 4,
            Year = 5
        }
    }
}
