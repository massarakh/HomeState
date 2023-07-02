using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NLog;
using TG_Bot.BusinessLayer.Abstract;
using TG_Bot.Helpers;

namespace TG_Bot.BusinessLayer.Concrete
{
    public class HealthService : BackgroundService, IDisposable, IHealthService
    {
        private Task _executingTask;

        private readonly CancellationTokenSource _stoppingCts =
            new CancellationTokenSource();

        private CancellationToken Token => _stoppingCts.Token;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly BotHelper _botHelper;
        private readonly IConfiguration configuration;
        private readonly IRestService restService;

        private List<string> Ttys
        {
            get
            {
                IEnumerable<IConfigurationSection> sections = configuration.GetSection("Tty").GetChildren();
                var arguments = (from s in sections
                                 select s.Value).ToList();

                return arguments;
            }
        }

        public HealthService(IConfiguration configuration, IRestService restService)
        {
            _botHelper = new BotHelper(configuration);
            this.configuration = configuration;
            this.restService = restService;
        }


        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => { _logger.Info($"Health service stopping"); }, true);
            _executingTask = new Task(() =>
            {
                if (Ttys.Count == 0)
                {
                    return;
                }
                while (!cancellationToken.IsCancellationRequested)
                {
                    var cancelled = cancellationToken.WaitHandle.WaitOne(10000);
                    if (cancelled)
                        break;
                    _logger.Info("Check");
                    bool status = Check();
                    if (!status)
                    {
                        int count = 0;
                        while (status != true || count <= Ttys.Count)
                        {
                            Reconfigure(Ttys[count]);
                            cancellationToken.WaitHandle.WaitOne(5000);
                            status = Check();
                            count++;
                        }
                    }
                }
            }, cancellationToken);

            try
            {
                _executingTask.Start();
                _logger.Debug("Health service initiated");
            }
            catch (OperationCanceledException)
            {
                _logger.Debug($"Health service cancelled");
            }

            await _executingTask;

        }

        ///// <inheritdoc />
        //public async Task StopAsync(CancellationToken cancellationToken)
        //{
        //    if (_executingTask == null)
        //    {
        //        return;
        //    }

        //    try
        //    {
        //        _logger.Debug($"Try to stop health service");
        //        _stoppingCts.Cancel();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Debug($"Error while stopping - {ex.Message}");
        //    }

        //    await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));

        //    cancellationToken.ThrowIfCancellationRequested();
        //    _logger.Info("Health service stoppped");
        //}

        //// <inheritdoc />
        //public void Dispose()
        //{
        //    _executingTask.Dispose();
        //    _logger.Info("Dispose");
        //    _stoppingCts.Dispose();
        //}

        /// <inheritdoc />
        public bool Check()
        {
            return restService.CheckConnectivity();
        }

        /// <inheritdoc />
        public bool Reconfigure(string args)
        {
            string cmd = "sudo /home/pi/CCU/reconfigure.sh " + args;
            cmd.Bash();
            //string cmd = "uptime -p | cut -d \" \" -f2-";
            //var escapedArgs = cmd.Replace("\"", "\\\"");
            //string fileName = "/bin/bash";
            //string arguments = $"-c \"{escapedArgs}\"";
            //string result = string.Empty;
            //if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //{
            //    ProcessStartInfo procStartInfo =
            //        new ProcessStartInfo("cmd", cmd)
            //        {
            //            FileName = fileName,
            //            Arguments = arguments,
            //            RedirectStandardOutput = true,
            //            UseShellExecute = false,
            //            CreateNoWindow = true
            //        };

            //    Process proc = new Process
            //    {
            //        StartInfo = procStartInfo
            //    };
            //    proc.Start();
            //    result = proc.StandardOutput.ReadToEnd();
            //    proc.WaitForExit();
            //}


            //sudo /home/pi/CCU/reconfigure.sh "ARG1=/dev/ttyACM0"
            //etc/.ccuargconf
            //ARG1=/dev/ttyACM0
            /// Остановить службу
            /// запомнить текущее значение
            /// перезаписать новым
            /// проверить
            return true;
        }
    }
}