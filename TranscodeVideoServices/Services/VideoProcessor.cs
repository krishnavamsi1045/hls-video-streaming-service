using System.Diagnostics;
using TranscodeVideoServices.Models;

namespace TranscodeVideoServices.Services
{
    public interface IVideoProcessor
    {
        Task<VideoProcessingResult> ProcessAsync(string inputPath, string outputPath);
    }
    public class FFmpegVideoProcessor : IVideoProcessor
    {
        public async Task<VideoProcessingResult> ProcessAsync(string inputPath, string outputPath)
        {
            var profiles = new List<VideoVarintPofile> {
                new (){Resolution = "1080p",Scale="1920:1080",Bitrate="3000k"},
                new (){Resolution="720p",Scale="1280:720",Bitrate="1500k"},
                new (){Resolution="480p",Scale = "854:480",Bitrate="800k"}
            };

            var args = BuildFfmpegArgs(inputPath, outputPath, profiles);
            await RunFFmpeg(args);
            var varints = new List<VarintInfo>();

            for (int i = 0; i < profiles.Count; i++)
            {
                varints.Add(new VarintInfo
                {
                    Resolution = profiles[i].Resolution,
                    Bitrate = int.Parse(profiles[i].Bitrate.Replace("k", "")),
                    PlayListPath = $"{outputPath}/output_{i}/index.m3u8"
                }); ;
            }

            return new VideoProcessingResult
            {
                MasterPlayListPath = $"{outputPath}/master.m3u8",
                Varints = varints
            };


        }

        private string BuildFfmpegArgs(string input, string outputDir, List<VideoVarintPofile> profiles)
        {
            var args = $"-i \"{input}\" ";

            for (int i = 0; i < profiles.Count; i++)
            {
                args += $"-filter:v:{i} scale={profiles[i].Scale} -b:v:{i} {profiles[i].Bitrate} ";
            }

            args += "-map 0:v -map 0:a? ";
            args += "-f hls -hls_time 6 -hls_playlist_type vod ";
            args += "-master_pl_name master.m3u8 ";

            // map variants
            var map = string.Join(" ", profiles.Select((p, i) => $"v:{i},a:0"));
            args += $"-var_stream_map \"{map}\" ";

            args += $"\"{Path.Combine(outputDir, "output_%v/index.m3u8")}\"";

            return args;
        }


        public async Task RunFFmpeg(string arg)
        {
            var process = new Process();

            process.StartInfo.FileName = "ffmpeg";
            process.StartInfo.Arguments = arg;

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;

            process.Start();

            string error = await process.StandardError.ReadToEndAsync();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception(error);
            }


        }

    }
}
