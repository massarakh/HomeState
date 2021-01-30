using System.Threading.Tasks;
using TG_Bot.BusinessLayer.Abstract;
using TG_Bot.DAL;
using TG_Bot.Helpers;

namespace TG_Bot.BusinessLayer.Concrete
{
    public class StateService : IStateService
    {
        private readonly IStateRepository _repository;

        public StateService(IStateRepository repository)
        {
            _repository = repository;
        }
        /// <inheritdoc />
        public async Task<string> LastState()
        {
            var state = await _repository.GetState();
            return $"<pre>Время:           {state.Timestamp}\n" +
                   $"Фаза 1:          {state.Electricity.Phase1} А\n" +
                   $"Фаза 2:          {state.Electricity.Phase2} A\n" +
                   $"Фаза 3:          {state.Electricity.Phase3} A\n" +
                   $"Сумма фаз:       {state.Electricity.PhaseSumm} A\n" +
                   $"Бойлер, питание: {state.Boiler.ToFormatted()}\n" +
                   $"Тёплые полы:     {state.Heat.Floor.ToFormatted()}\n" +
                   $"Батареи:         {state.Heat.Batteries.ToFormatted()}\n" +
                   $"Гостиная (t°):   {state.Temperature.LivingRoom} °С\n" +
                   $"Гостиная (%):    {state.Humidity.LivingRoom} %\n" +
                   $"Спальня (t°):    {state.Temperature.Bedroom} °С\n" +
                   $"Спальня (%):     {state.Humidity.Bedroom} %\n" +
                   $"Сарай (t°):      {state.Temperature.Barn} °С\n" +
                   $"Улица (t°):      {state.Temperature.Outside} °С\n" +
                   $"Энергия:         {state.Energy} кВт⋅ч</pre>";
        }

        /// <inheritdoc />
        public async Task<string> Electricity()
        {
            var state = await _repository.GetState();
            return $"<pre>Время:     {state.Timestamp}\n" +
                   $"Фаза 1:    {state.Electricity.Phase1} А\n" +
                   $"Фаза 2:    {state.Electricity.Phase2} A\n" +
                   $"Фаза 3:    {state.Electricity.Phase3} A\n" +
                   $"Сумма фаз: {state.Electricity.PhaseSumm} A</pre>\n";
        }

        /// <inheritdoc />
        public async Task<string> Temperature()
        {
            var state = await _repository.GetState();
            return $"<pre>Время:         {state.Timestamp}\n" +
                   $"Гостиная (t°): {state.Temperature.LivingRoom} °С\n" +
                   $"Гостиная (%):  {state.Humidity.LivingRoom} %\n" +
                   $"Спальня (t°):  {state.Temperature.Bedroom} °С\n" +
                   $"Спальня (%):   {state.Humidity.Bedroom} %\n" +
                   $"Сарай (t°):    {state.Temperature.Barn} °С\n" +
                   $"Улица (t°):    {state.Temperature.Outside} °С</pre>\n";

        }

        /// <inheritdoc />
        public async Task<string> Heating()
        {
            var state = await _repository.GetState();
            return $"<pre>Бойлер:        {state.Boiler.ToFormatted()}\n" +
                   $"Тёплые полы:   {state.Heat.Floor.ToFormatted()}\n" +
                   $"Батареи:       {state.Heat.Batteries.ToFormatted()}</pre>\n";
        }

        //private bool BoilerHeat(Data data)
        //{
        //    if (data.Boiler && data.Electricity.Phase3)
        //}

        //[NotMapped]
        //public string HeatFloor
        //{
        //    get
        //    {
        //        int valHeat = Convert.ToInt32(Heat);
        //        if (valHeat == 3 || valHeat == 9)
        //        {
        //            //return new SingleEmoji(new UnicodeSequence("1F7E2"), "green circle", new[] { "green", "circle" }, 1).ToString();
        //            return "Вкл.";
        //        }
        //        //return new SingleEmoji(new UnicodeSequence("1F534"), "red circle", new[] { "red", "circle" }, 1).ToString();
        //        return "Выкл.";
        //    }
        //}

        //[NotMapped]
        //public string HeatBatteries
        //{
        //    get
        //    {
        //        int valHeat = Convert.ToInt32(Heat);
        //        if (valHeat == 6 || valHeat == 9)
        //        {
        //            return "Вкл.";
        //            //return new SingleEmoji(new UnicodeSequence("1F7E2"), "green circle", new[] { "green", "circle" }, 1).ToString();
        //        }
        //        //return new SingleEmoji(new UnicodeSequence("1F534"), "red circle", new[] { "red", "circle" }, 1).ToString();
        //        return "Выкл.";
        //    }
        //}

        //[NotMapped]
        //public string BoilerState => Convert.ToInt32(Boiler) == 0 ? "Выкл." : "Вкл.";
        ////public string BoilerState => Convert.ToInt32(Boiler) == 0 ?
        ////    new SingleEmoji(new UnicodeSequence("1F534"), "red circle", new[] { "red", "circle" }, 1).ToString()
        ////    : new SingleEmoji(new UnicodeSequence("1F7E2"), "green circle", new[] { "green", "circle" }, 1).ToString();

    }
}
