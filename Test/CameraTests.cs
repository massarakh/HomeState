using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Test
{
    public class CameraTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public CameraTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task GetPicture()
        {
            //rtsp://admin:123456@192.168.0.9:554/mpeg4
            //http://192.168.0.9/cgi-bin/snapshot.cgi?stream=0

            string codeBase = Assembly.GetExecutingAssembly().Location;
            UriBuilder uri = new UriBuilder(codeBase);
            string tmp = Uri.UnescapeDataString(uri.Path);
            string dirName = "frames";
            var PathDir = Path.GetDirectoryName(tmp);
            PathDir = System.IO.Path.Combine(PathDir, dirName);
            string localFilename = DateTime.Now.ToString("O").Replace(":", "_") + ".jpg";
            PathDir = Path.Combine(PathDir, localFilename);
            using (WebClient client = new WebClient())
            {
                client.DownloadFile("http://192.168.0.9/cgi-bin/snapshot.cgi?stream=0", PathDir);
            }

            Assert.True(File.Exists(PathDir));

        }

        [Fact]
        public void FfmpegInstalled()
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo("cmd", "/c " + "ffmpeg -version")
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                // Do not create the black window.
                // Now we create a process, assign its ProcessStartInfo and start it
                Process proc = new System.Diagnostics.Process
                {
                    StartInfo = procStartInfo
                };
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadLine();
                // Display the command output.
                _testOutputHelper.WriteLine(result);
                //"ffmpeg version 4.2.2 Copyright (c) 2000-2019 the FFmpeg developers"
                Assert.True(result.Contains("ffmpeg version"));
            }
            catch (Exception objException)
            {
                _testOutputHelper.WriteLine($"Error - {objException.Message}");
            }
        }

        [Fact]
        public void GetFfmegPicture()
        {
            string path = Path.GetTempPath() + "cam2.jpg";
            ProcessStartInfo procStartInfo =
                new System.Diagnostics.ProcessStartInfo("cmd", "/c " +
                   "ffmpeg -i rtsp://192.168.0.10:554/user=admin_password=tlJwpbo6_channel=1_stream=0.sdp?real_stream -r 1 -f image2 -frames:v 1 " + path)
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
            //string result = proc.StandardOutput.ReadLine();
            //_testOutputHelper.WriteLine(result);

            Assert.True(File.Exists(path));
            try
            {
                File.Delete(path);
            }
            catch
            {
            }
        }
    }
}
