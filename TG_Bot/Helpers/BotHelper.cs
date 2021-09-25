using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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

        /// <summary>
        /// Получение токена из файла
        /// </summary>
        /// <returns></returns>
        private string GetBotTokenFromFile()
        {
            var path = Directory.GetParent(AppContext.BaseDirectory).FullName;
            var PathToken = Path.Combine(path, "BotToken.txt");
            if (!File.Exists(PathToken))
                return null;

            return File.ReadAllText(PathToken);
        }

        /// <summary>
        /// Получение токена телеграм бота
        /// </summary>
        public string GetBotToken()
        {
            string _botToken;
            IEnumerable<IConfigurationSection> sections = _configuration.GetSection("BotConfiguration").GetChildren();
            var tokenSection = sections.FirstOrDefault(_ => _.Key == "BotToken");
            if (tokenSection == null)
            {
                _logger.Error($"Не найден токен для бота, выход");
                return string.Empty;
            }

            switch (tokenSection.Value)
            {
                case "<BotToken>":
                    {
                        string token = GetBotTokenFromFile();
                        _botToken = token;
                        return string.IsNullOrEmpty(token) ? string.Empty : token;
                    }

                default:
                    _botToken = tokenSection.Value;
                    return _botToken;
            }

        }

        /// <summary>
        /// Получить состояние системы
        /// </summary>
        /// <returns></returns>
        public string GetSystemInfo()
        {
            try
            {
                string result = string.Empty;
                var assembly = Assembly.GetExecutingAssembly();
                var attribute = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0] as AssemblyDescriptionAttribute;
                string description = attribute?.Description;

                if (!string.IsNullOrEmpty(description))
                {
                    result += description+"\n";
                }

                result += $"Время работы: {GetUptimeValue()}";

                //_logger.Info($"Сборка {assembly?.FullName}");
                //string version = assembly?.GetName().Version.ToString();
                //_logger.Info($"Версия сборки");
                //string filePath = assembly?.Location;
                //_logger.Info($"Местоположение сборки {filePath}");
                //DateTime dt = new FileInfo(filePath).LastWriteTime;
                //string uptime = GetUptimeValue();
                //string result = $"Версия бота: {version}\n" +
                //                $"Дата сборки: {dt:d'.'MM'.'yy}\n";
                //if (!string.IsNullOrEmpty(uptime))
                //{
                //    string uptimeString = $"Время работы: {uptime}";
                //    return result + uptimeString;
                //}
                //return result;
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка запроса, {ex.Message}");
                return "Ошибка запроса системной информации";
            }
        }

        /// <summary>
        /// Возврат количества времени uptime на линукс-системе
        /// </summary>
        /// <returns></returns>
        private string GetUptimeValue()
        {
            string cmd = "uptime -p | cut -d \" \" -f2-";
            var escapedArgs = cmd.Replace("\"", "\\\"");
            string fileName = "/bin/bash";
            string arguments = $"-c \"{escapedArgs}\"";
            string result = string.Empty;
            if (!IsWindows())
            {
                ProcessStartInfo procStartInfo =
                    new ProcessStartInfo("cmd", cmd)
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                Process proc = new Process
                {
                    StartInfo = procStartInfo
                };
                proc.Start();
                result = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
            }

            return result;
        }

        /// <summary>
        /// Ключевая ОС
        /// </summary>
        /// <returns></returns>
        public bool IsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

    }
}