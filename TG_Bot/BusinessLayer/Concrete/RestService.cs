using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using TG_Bot.BusinessLayer.Abstract;
using TG_Bot.BusinessLayer.CCUModels;

namespace TG_Bot.BusinessLayer.Concrete
{
    public class RestService : IRestService
    {

        private readonly ILogger<RestService> _logger;
        private IConfiguration _configuration { get; }

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
                _logger.LogError($"Не найден адрес контроллера, выход");
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
                _logger.LogError($"Не найден логин для авторизации на контроллере, выход");
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
                _logger.LogError($"Не найден пароль для авторизации на контроллере, выход");
                return string.Empty;
            }
        }

        public RestService(IConfiguration configuration, ILogger<RestService> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }
        /// <inheritdoc />
        public string SwitchOutput(Output output)
        {
            string result = string.Empty;
            var client = new RestClient(Url)
            {
                Authenticator = new HttpBasicAuthenticator(Login, Password)
            };
            var cmd = new CommandRequest { Command = "SetOutputState", Number = output.Number, State = output.State };

            var request = new RestRequest().AddParameter("cmd", JsonConvert.SerializeObject(cmd));
            var response = client.Get(request);
            var model = JsonConvert.DeserializeObject<CcuState>(response.Content);

            return result;
        }

        /// <inheritdoc />
        public string GetState()
        {
            throw new System.NotImplementedException();
            
        }
    }
}