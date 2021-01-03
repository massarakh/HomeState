using System.IO;

namespace TG_Bot.BusinessLayer
{
    public interface ICamService
    {
        /// <summary>
        /// Получение изображения с камеры въезда
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Поток</returns>
        MemoryStream GetEntranceCam(out string fileName);

        /// <summary>
        /// Получение изображения с камеры двора
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Поток</returns>
        FileStream GetYardCam(out string fileName);
    }
}