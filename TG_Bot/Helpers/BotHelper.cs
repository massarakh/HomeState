using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NLog;

namespace TG_Bot.Helpers
{
    public class BotHelper
    {
        private readonly IConfiguration _configuration;
        private List<int> _authorizedIds = new List<int>();
        private DateTime LastSynced = DateTime.MinValue;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public BotHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Проверка наличия ключа в списке разрешённых пользователей
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsAuthorized(int id)
        {
            DateTime dt = DateTime.Now;

            if (dt.AddMinutes(-5) > LastSynced)
            {
                LastSynced = DateTime.Now;
                try
                {
                    _authorizedIds = _configuration.GetSection("AllowedUsersIds")
                                .AsEnumerable()
                                .Select(x =>
                                    string.IsNullOrEmpty(x.Value) ? 0 : Convert.ToInt32(x.Value))
                                .ToList();
                    _authorizedIds.RemoveAll(_ => _ == 0);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Ошибка разбора авторизованных ID, авторизация не пройдена - {ex.Message}");
                    return false;
                }
            }

            return _authorizedIds.Contains(id);
        }
    }
}