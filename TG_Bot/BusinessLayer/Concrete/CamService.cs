using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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
        /// Адрес для подключения к камере обзора
        /// </summary>
        private string OverviewCam
        {
            get
            {
                IEnumerable<IConfigurationSection> sections = _configuration.GetSection("OverviewCam").GetChildren();
                var cam = sections.FirstOrDefault(_ => _.Key == "rtsp");
                if (cam != null) return cam.Value;
                _logger.Error($"Не найден адрес для камеры обзора, выход");
                return string.Empty;

            }
        }

        /// <summary>
        /// Имя для файла со снимком
        /// </summary>
        private string CamFileName => DateTime.Now.ToString("H'_'mm'_'ss") + ".jpg";

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

        /// <inheritdoc />
        public async Task<Tuple<string, string>> GetFfmpegCam(CancellationToken stoppingCtsToken, string camName)
        {
            string fileNameToSave = camName + "_" + CamFileName;
            string pathToSave = Path.Combine(Path.GetTempPath(), fileNameToSave);
            string cmd;
            if (camName.ToLower().Contains("yard"))
            {
                if (IsWindows())
                {
                    cmd = "/c " + YardCam + " \"" + pathToSave + "\"";
                }
                else
                {
                    cmd = YardCam + " \"" + pathToSave + "\"";
                }
            }
            else
            {
                if (IsWindows())
                {
                    cmd = "/c " + OverviewCam + " \"" + pathToSave + "\"";
                }
                else
                {
                    cmd = OverviewCam + " \"" + pathToSave + "\"";
                }
            }
            _logger.Debug($"Команда запроса - {cmd}");
            try
            {
                Task<int> task = Task.Run(() =>
                {
                    stoppingCtsToken.ThrowIfCancellationRequested();
                    string fileName;
                    string arguments;
                    if (IsWindows())
                    {
                        fileName = "cmd";
                        arguments = cmd;
                    }
                    else
                    {
                        var escapedArgs = cmd.Replace("\"", "\\\"");
                        fileName = "/bin/bash";
                        arguments = $"-c \"{escapedArgs}\"";
                    }

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
                    proc.WaitForExit(20000);
                    return proc.ExitCode;
                }, stoppingCtsToken);
                await Task.WhenAny(task, Task.Delay(-1, stoppingCtsToken));
                _logger.Debug($"Запрос изображения завершился с кодом {task.Result}");
                if (!File.Exists(pathToSave))
                {
                    throw new Exception($"Изображение не сохранено с помощью консольной программы ffmpeg");
                }
                return new Tuple<string, string>(pathToSave, fileNameToSave);
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось получить изображение с камеры - {ex.Message}");
            }
        }

        /// <summary>
        /// Ключевая ОС
        /// </summary>
        /// <returns></returns>
        private bool IsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        /// <summary>
        /// Проверка установленного ffmpeg
        /// </summary>
        private bool CheckFFmpegInstalled()
        {
            string fileName = IsWindows() ? "cmd" : "/bin/bash";
            string arguments = IsWindows() ? "/c " + "ffmpeg -version" : "-c ffmpeg -version";
            ProcessStartInfo procStartInfo =
                new ProcessStartInfo()
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
            proc.WaitForExit();
            var output = proc.StandardOutput.ReadLine();

            string result = new string(output?.Take(21).ToArray());

            if (!string.IsNullOrEmpty(result) && result.Contains("ffmpeg version"))
            {
                _logger.Info(result);
                return true;
            }
            return false;
        }
    }
}