using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TG_Bot.BusinessLayer;
using TG_Bot.monitoring;

namespace TG_Bot.DAL
{
    public interface IStateRepository : IRepositoryBase<Monitor>
    {
        /// <summary>
        /// Получение всех последних параметров
        /// </summary>
        /// <returns></returns>
        Task<Data> GetState();

        /// <summary>
        /// Получить последние показания электричества
        /// </summary>
        /// <returns></returns>
        Task<Electricity> GetElectricity();

        /// <summary>
        /// Получить последние показания по обогреванию
        /// </summary>
        /// <returns></returns>
        Task<Heat> GetHeating();

        /// <summary>
        /// Получить последние показания по температурам
        /// </summary>
        /// <returns></returns>
        Task<Temperature> GetTemperatures();

        /// <summary>
        /// Получить последние показания по влажности
        /// </summary>
        /// <returns></returns>
        Task<Humidity> GetHumidity();
    }
}
