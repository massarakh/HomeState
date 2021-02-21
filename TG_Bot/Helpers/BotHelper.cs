using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace TG_Bot.Helpers
{
    public class BotHelper
    {
        private readonly IConfiguration _configuration;
        private List<int> _authorizedIds = new List<int>();
        private DateTime LastSynced = DateTime.MinValue;

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
                _authorizedIds = _configuration.GetSection("AllowedUsersIds")
                    .AsEnumerable()
                    .Select(x => Convert.ToInt32(x.Value))
                    //.Where(x => x != null)
                    .ToList();
            }

            return _authorizedIds.Contains(id);
        }
    }
}