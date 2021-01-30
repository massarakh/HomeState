using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NLog;
using TG_Bot.BusinessLayer.Abstract;

namespace TG_Bot.BusinessLayer.Concrete
{
    public class CamService : ICamService
    {

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IConfiguration _configuration { get; }

        /// <summary>
        /// Адрес для получения изображения с камеры въезда
        /// </summary>
        private string EntranceCam
        {
            get
            {
                IEnumerable<IConfigurationSection> sections = _configuration.GetSection("EntranceCam").GetChildren();
                var cam = sections.FirstOrDefault(_ => _.Key == "snapshot");
                if (cam != null) return cam.Value;
                _logger.Error($"Не найден адрес для камеры въезда, выход");
                return string.Empty;

            }
        }

        /// <summary>
        /// Адрес для подключения к камере двора
        /// </summary>
        private string YardCam
        {
            get
            {
                IEnumerable<IConfigurationSection> sections = _configuration.GetSection("YardCam").GetChildren();
                var cam = sections.FirstOrDefault(_ => _.Key == "rtsp");
                if (cam != null) return cam.Value;
                _logger.Error($"Не найден адрес для камеры двора, выход");
                return string.Empty;

            }
        }

        /// <summary>
        /// Имя для файла со снимком
        /// </summary>
        private string CamFileName => DateTime.Now.ToString("H'_'mm'_'ss d MMM yyyy") + ".jpg";

        public CamService(IConfiguration configuration)
        {
            _configuration = configuration;
            if (!CheckFFmpegInstalled())
            {
                _logger.Fatal($"Не найден ffpeg в системе");
            }

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

            return pathToSave;
        }

        /// <param name="stoppingCtsToken"></param>
        /// <inheritdoc />
        public async Task<Tuple<string, string>> GetYardCam(CancellationToken stoppingCtsToken)
        {
            string fileNameToSave = "YardCam_" + CamFileName;
            string pathToSave = Path.Combine(Path.GetTempPath(), fileNameToSave);
            string cmd = "/c " + YardCam + "\"" + pathToSave + "\"";
            _logger.Debug($"Команда запроса - {cmd}");
            try
            {
                Task<int> task = Task.Run(() =>
                {
                    stoppingCtsToken.ThrowIfCancellationRequested();
                    ProcessStartInfo procStartInfo =
                        new ProcessStartInfo("cmd", cmd)
                        {
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                    Process proc = new Process
                    {
                        StartInfo = procStartInfo
                    };
                    proc.Start();
                    proc.WaitForExit();
                    return proc.ExitCode;
                }, stoppingCtsToken);
                await Task.WhenAny(task, Task.Delay(-1, stoppingCtsToken));
                _logger.Debug($"Запрос изображения завершился с кодом {task}");
                if (!File.Exists(pathToSave))
                {
                    throw new Exception($"Изображение не сохранено с помощью консольной программы ffmpeg");
                }
                return new Tuple<string, string>(pathToSave, fileNameToSave);
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось получить изображение с камеры двора - {ex.Message}");
            }
        }

        /// <summary>
        /// Проверка установленного ffmpeg
        /// </summary>
        private bool CheckFFmpegInstalled()
        {
            ProcessStartInfo procStartInfo =
                new System.Diagnostics.ProcessStartInfo("cmd", "/c " + "ffmpeg -version")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

            Process proc = new System.Diagnostics.Process
            {
                StartInfo = procStartInfo
            };
            proc.Start();
            string result = proc.StandardOutput.ReadLine();

            if (result != null && result.Contains("ffmpeg version"))
            {
                _logger.Info(result);
                return true;
            }
            //_logger.LogError($"Не найден ffmpeg в системе");
            return false;
        }
    }
}