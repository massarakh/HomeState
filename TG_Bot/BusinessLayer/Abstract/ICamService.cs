using System;
using System.Threading;
using System.Threading.Tasks;

namespace TG_Bot.BusinessLayer.Abstract
{
    public interface ICamService
    {
        /// <summary>
        /// Получение изображения с камеры въезда
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Поток</returns>
        string GetEntranceCam(out string fileName);

        /// <summary>
        /// Получение изображения с камеры двора
        /// </summary>
        /// <param name="stoppingCtsToken"></param>
        /// <returns>Поток</returns>
        Task<Tuple<string, string>> GetYardCam(CancellationToken stoppingCtsToken);
    }
}