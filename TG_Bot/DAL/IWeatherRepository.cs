using System.Threading.Tasks;
using TG_Bot.BusinessLayer.Concrete;

namespace TG_Bot.DAL
{
    public interface IWeatherRepository:IRepositoryBase<Openweather>
    {
        /// <summary>
        /// Получение погоды
        /// </summary>
        /// <returns></returns>
        Task<Weather> GetLastWeather();
    }
}