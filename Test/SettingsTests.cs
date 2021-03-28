using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Test
{
    public class SettingsTests
    {
        [Fact]
        public void GetSettings()
        {
            var path = Directory.GetParent(AppContext.BaseDirectory).FullName;
            string baseDir = path.Substring(0, path.LastIndexOf("Test", StringComparison.Ordinal));
            string appsettingsPath = Path.Combine(baseDir, "TG_Bot");

            var _configuration = new ConfigurationBuilder()
                .SetBasePath(appsettingsPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var val = _configuration.GetSection("AllowedUsersIds")
                .AsEnumerable()
                .Select(x => x.Value)
                .Where(x => x != null);
            Assert.NotNull(val);

            //switch (val)
            //{
            //    //case "<UserIds>":
            //    //    {
            //    //        string token = GetBotTokenFromFile();
            //    //        _botToken = token;
            //    //        return string.IsNullOrEmpty(token) ? string.Empty : token;
            //    //    }

            //    default:
            //        _botToken = tokenSection.Value;
            //        return _botToken;
            //}
        }
    }
}