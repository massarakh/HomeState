using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RtspClientSharp;
using RtspClientSharp.RawFrames.Audio;
using RtspClientSharp.RawFrames.Video;
using Xunit;
using System.IO;
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


            //CancellationTokenSource cts = new CancellationTokenSource();
            //try
            //{
            //    string codeBase = Assembly.GetExecutingAssembly().Location;
            //    UriBuilder uri = new UriBuilder(codeBase);
            //    string tmp = Uri.UnescapeDataString(uri.Path);
            //    string dirName = "frames";
            //    var PathDir = Path.GetDirectoryName(tmp);
            //    PathDir = System.IO.Path.Combine(PathDir, dirName);
            //    if (!Directory.Exists(PathDir))
            //        Directory.CreateDirectory(PathDir);

            //    //int intervalMs = options.Interval * 1000;
            //    //int lastTimeSnapshotSaved = Environment.TickCount - intervalMs;

            //    var connectionParameters = new ConnectionParameters(new Uri("rtsp://admin:123456@192.168.0.9:554/mpeg4"), new NetworkCredential("admin", "123456"));
            //    using (var rtspClient = new RtspClient(connectionParameters))
            //    {
            //        rtspClient.FrameReceived += (sender, frame) =>
            //        {
            //            if (!(frame is RawJpegFrame))
            //                return;

            //            string snapshotName = frame.Timestamp.ToString("O").Replace(":", "_") + ".jpg";
            //            string path = Path.Combine(PathDir, snapshotName);

            //            ArraySegment<byte> frameSegment = frame.FrameSegment;

            //            using (var stream = File.OpenWrite(path))
            //                stream.Write(frameSegment.Array, frameSegment.Offset, frameSegment.Count);

            //            _testOutputHelper.WriteLine($"[{DateTime.UtcNow}] Snapshot is saved to {snapshotName}");
            //        };

            //        _testOutputHelper.WriteLine("Connecting...");
            //        await rtspClient.ConnectAsync(cts.Token);
            //        _testOutputHelper.WriteLine("Receiving...");
            //        await rtspClient.ReceiveAsync(cts.Token);
            //    }
            //}
            //catch (OperationCanceledException)
            //{
            //}
            //catch (Exception e)
            //{
            //    _testOutputHelper.WriteLine(e.ToString());
            //}
            //Assert.True(true);
        }
    }
}
