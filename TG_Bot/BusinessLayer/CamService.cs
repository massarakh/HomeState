using System;
using System.IO;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace TG_Bot.BusinessLayer
{
    public class CamService : ICamService
    {
        private IConfiguration _configuration { get; }

        public string EntranceCam
        {
            get
            {
                var t = _configuration["EntranceCam"];
                return string.Empty;
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

        public CamService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        /// <inheritdoc />
        public MemoryStream GetEntranceCam(out string fileName)
        {
            // Определение пути
            string fileNameToSave = "EntranceCam_" + CamFileName;
            fileName = fileNameToSave;
            string pathToSave = Path.Combine(LocationToSave, fileName);

            // Сохранение в файл
            try
            {
                using WebClient client = new WebClient();
                client.DownloadFile(EntranceCam, pathToSave);
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
            MemoryStream destination = new MemoryStream();
            using (FileStream fs = new FileStream(pathToSave, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fs.CopyTo(destination);
            }

            return destination;
        }

        /// <inheritdoc />
        public FileStream GetYardCam(out string fileName)
        {
            throw new System.NotImplementedException();
        }
    }
}