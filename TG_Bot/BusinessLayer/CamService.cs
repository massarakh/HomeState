﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
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
                if (cam != null) return cam.Value;
                _logger.LogError($"Не найден адрес для камеры въезда, выход");
                return string.Empty;

            }
        }

        private string YardCam
        {
            get
            {
                IEnumerable<IConfigurationSection> sections = _configuration.GetSection("YardCam").GetChildren();
                var cam = sections.FirstOrDefault(_ => _.Key == "rtsp");
                if (cam != null) return cam.Value;
                _logger.LogError($"Не найден адрес для камеры двора, выход");
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

        private string CamFileName => DateTime.Now.ToString("H'_'mm'_'ss d MMM yyyy") + ".jpg";

        public CamService(IConfiguration configuration, ILogger<CamService> logger)
        {
            _logger = logger;
            _configuration = configuration;
            if (!CheckFFmpegInstalled())
            {
                _logger.LogCritical($"Не найден ffpeg в системе");
                //throw new Exception($"Не найден ffpeg в системе");
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

            // Отдача потока и имени
            //MemoryStream destination = new MemoryStream();
            //using (FileStream fs = new FileStream(pathToSave, FileMode.Open, FileAccess.Read, FileShare.Read))
            //{
            //    fs.CopyTo(destination);
            //}

            return pathToSave;
        }

        /// <inheritdoc />
        public async Task<Tuple<string, string>> GetYardCam()
        {
            string fileNameToSave = "YardCam_" + CamFileName;
            string pathToSave = Path.Combine(Path.GetTempPath(), fileNameToSave);
            string cmd = "/c " + YardCam + "\"" + pathToSave + "\"";
            _logger.LogDebug($"Команда запроса - {cmd}");
            try
            {
                Task<int> task = Task.Run(() =>
                {
                    ProcessStartInfo procStartInfo =
                        new System.Diagnostics.ProcessStartInfo("cmd", cmd)
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
                });
                await Task.WhenAny(task, Task.Delay(-1));
                _logger.LogDebug($"Запрос изображения завершился с кодом {task}");
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
                _logger.LogInformation(result);
                return true;
            }
            //_logger.LogError($"Не найден ffmpeg в системе");
            return false;
        }
    }
}