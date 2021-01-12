using TG_Bot.BusinessLayer.CCUModels;

namespace TG_Bot.BusinessLayer.Abstract
{
    /// <summary>
    /// Работа с API контроллера
    /// </summary>
    public interface IRestService
    {
        /// <summary>
        /// Переключение выхода
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        string SwitchOutput(Output output);

        /// <summary>
        /// Получение состояния
        /// </summary>
        /// <returns></returns>
        string GetState();
    }
}