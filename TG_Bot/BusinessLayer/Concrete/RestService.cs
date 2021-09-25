using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using RestSharp.Authenticators;
using TG_Bot.BusinessLayer.Abstract;
using TG_Bot.BusinessLayer.CCUModels;
using TG_Bot.Helpers;

namespace TG_Bot.BusinessLayer.Concrete
{
    public class RestService : IRestService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IConfiguration _configuration { get; }

        /// <summary>
        /// Команда переключения состояния
        /// </summary>
        /// <remarks>Временная мера размещения в RestService</remarks>
        public const string SwitchCommand = "SetOutputState";

        /// <summary>
        /// Команда получения состояния контроллера
        /// </summary>
        /// <remarks>Временная мера размещения в RestService</remarks>
        public const string GetStateCommand = "GetStateAndEvents";

        /// <summary>
        /// Адрес контролллера
        /// </summary>
        private string Url
        {
            get
            {
                IEnumerable<IConfigurationSection> sections = _configuration.GetSection("ControllerUrl").GetChildren();
                var url = sections.FirstOrDefault(_ => _.Key == "Url");
                if (url != null) return url.Value;
                _logger.Error($"Не найден адрес контроллера, выход");
                return string.Empty;
            }
        }

        /// <summary>
        /// Логин для авторизации на контроллере
        /// </summary>
        private string Login
        {
            get
            {
                IEnumerable<IConfigurationSection> sections = _configuration.GetSection("ControllerUrl").GetChildren();
                var login = sections.FirstOrDefault(_ => _.Key == "Login");
                if (login != null) return login.Value;
                _logger.Error($"Не найден логин для авторизации на контроллере, выход");
                return string.Empty;
            }
        }

        /// <summary>
        /// Логин для авторизации на контроллере
        /// </summary>
        private string Password
        {
            get
            {
                IEnumerable<IConfigurationSection> sections = _configuration.GetSection("ControllerUrl").GetChildren();
                var pwd = sections.FirstOrDefault(_ => _.Key == "Password");
                if (pwd != null) return pwd.Value;
                _logger.Error($"Не найден пароль для авторизации на контроллере, выход");
                return string.Empty;
            }
        }

        public RestService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        /// <inheritdoc />
        public string SwitchOutput(CommandRequest request)
        {
            CcuState model;
            try
            {
                var client = new RestClient(Url)
                {
                    Authenticator = new HttpBasicAuthenticator(Login, Password)
                };
                string param = JsonConvert.SerializeObject(request, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                });

                var req = new RestRequest().AddParameter("cmd", param);
                var response = client.Get(req);
                model = JsonConvert.DeserializeObject<CcuState>(response.Content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Невозможно изменить состояние выхода \"{request.Output.Name}\" - {ex.Message}");
            }

            return $"{request.Output.Name} - {model.Outputs[request.Output.Index].ToFormatted()}";
        }

        /// <inheritdoc />
        public string GetState()
        {
            CcuState model;
            try
            {
                var client = new RestClient(Url)
                {
                    Authenticator = new HttpBasicAuthenticator(Login, Password)
                };
                var cmd = new CommandRequest { Command = GetStateCommand };

                var request = new RestRequest().AddParameter("cmd", JsonConvert.SerializeObject(cmd, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }));
                var response = client.Get(request);
                model = JsonConvert.DeserializeObject<CcuState>(response.Content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Невозможно получить состояние контроллера - {ex.Message}");
            }
            return $"<pre>" +
                    $"Конвекторы:  {model.Outputs[0].ToFormatted()}\n" +
                    $"Бойлер:      {model.Outputs[2].ToFormatted()}\n" +
                    $"Полы (с/у):  {model.Outputs[3].ToFormatted()}\n" +
                    $"Спальня №4:  {model.Outputs[4].ToFormatted()}\n" +
                    $"Кухня:       {model.Outputs[5].ToFormatted()}\n" +
                    $"\nСостояние контроллера\n" +
                    $"Напряжение:  {model.Power} V\n" +
                    $"Температура: {model.Temp} °С\n" +
                    $"Баланс:      {model.Balance} ₽\n" +
                    $"Батарея:     {model.Battery.Charge}%</pre>";
        }
    }
}