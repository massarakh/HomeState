using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;

namespace TG_Bot.Helpers
{
    public static class ShellHelper
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static Task<int> Bash(this string cmd)
        {
            var source = new TaskCompletionSource<int>();
            var escapedArgs = cmd.Replace("\"", "\\\"");
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };
            process.Exited += (sender, args) =>
            {
                logger.Warn(process.StandardError.ReadToEnd());
                logger.Info(process.StandardOutput.ReadToEnd());
                if (process.ExitCode == 0)
                {
                    source.SetResult(0);
                }
                else
                {
                    source.SetException(new Exception($"Command `{cmd}` failed with exit code `{process.ExitCode}`"));
                }

                process.Dispose();
            };

            try
            {
                process.Start();
            }
            catch (Exception e)
            {
                logger.Error(e, "Command {} failed", cmd);
                source.SetException(e);
            }

            return source.Task;
        }

        /// <summary>
        /// Выполнить команду
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string ExecuteCommand(string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            string fileName = "/bin/bash";
            string arguments = $"-c \"{escapedArgs}\"";
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
            var result = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            return result;
        }

        public static string GetCurrentPort()
        {
            string cmd = "nano /etc/.ccuargconf";
            string res = ExecuteCommand(cmd);
            if (!string.IsNullOrEmpty(res))
                res = res.Replace("ARG1=", string.Empty);
            return res;
        }

        //public static Task TryToReconfigure(List<string> ttys, out string s)
        //{
        //    //Надо проверить запуск скрипта и запуск команды получения порта
        //    //добавить проверку на наличие файла reconfigure и файла с портом
        //    foreach (string tty in ttys)
        //    {
        //        try
        //        {
        //            string cmd = "sudo /home/pi/CCU/reconfigure.sh " + tty;
        //            cmd.Bash();

        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error($"Ошибка выполнения скрипта - {ex.Message}");
        //        }
        //    }


        //}
    }
}