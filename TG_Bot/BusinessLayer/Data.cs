using System;
using System.Collections.Generic;
using System.Text;

namespace TG_Bot.BusinessLayer
{
   public class Data
   {
       public string Timestamp;
       public Humidity Humidity;
       public Temperature Temperature;
       public Electricity Electricity;
       public Heat Heat;
       public bool Boiler;
       public bool BoilerHeat;
       public string Energy;
       public string Date;

       // Получается из CCU таблицы
       public bool BedroomYouth;
       public bool WarmFloorKitchen;
   }
}
