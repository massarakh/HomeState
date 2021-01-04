using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TG_Bot.BusinessLayer
{
    public class CamService : ICamService
    {
        private readonly ILogger<CamService> _logger;
        private IConfiguration _configuration { get; }

        private string EntranceCam
        {
            get
            {
                IEnumerable<IConfigurationSection> sections = _configuration.GetSection("EntranceCam").GetChildren();
                var cam = sections.FirstOrDefault(_ => _.Key == "snapshot");
                if (cam == null)
                {
                    _logger.LogError($"Не найден адрес для камеры въезда, выход");
                    return string.Empty;
                }

                return cam.Value;
            }
        }

        public string LocationToSave
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().Location;
                UriBuilder uri = new UriBuilder(codeBase);
                string tmp = Uri.UnescapeDataString(uri.Path);
                string dirName = "frames";
                var PathDir = Path.GetDirectoryName(tmp);
                return Path.Combine(PathDir, dirName);
            }
        }

        public string CamFileName => DateTime.Now.ToString("H'_'mm'_'ss d MMM yyyy") + ".jpg";

        public CamService(IConfiguration configuration, ILogger<CamService> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }
        /// <inheritdoc />
        public string GetEntranceCam(out string fileName)
        {
            // Определение пути
            string fileNameToSave = "EntranceCam_" + CamFileName;
            fileName = fileNameToSave;
            string pathToSave = Path.Combine(Path.GetTempPath(), fileNameToSave);
            // Сохранение в файл
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(EntranceCam, pathToSave);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось загрузить изображение с камеры въезда {ex.Message}");
            }

            if (!File.Exists(pathToSave))
            {
                throw new Exception($"Изображение с камеры въезда не найдено");
            }

            // Отдача потока и имени
            //MemoryStream destination = new MemoryStream();
            //using (FileStream fs = new FileStream(pathToSave, FileMode.Open, FileAccess.Read, FileShare.Read))
            //{
            //    fs.CopyTo(destination);
            //}

            return pathToSave;
        }

        /// <inheritdoc />
        public FileStream GetYardCam(out string fileName)
        {
            throw new System.NotImplementedException();
        }
    }
}