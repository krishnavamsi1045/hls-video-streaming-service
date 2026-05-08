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
        public async Task<VideoProcessingResult>
            ProcessAsync(
                string inputPath,
                string outputPath)
        {
            var args =
                BuildFfmpegArgs(
                    inputPath,
                    outputPath);

            await RunFFmpeg(args);

            return new VideoProcessingResult
            {
                MasterPlayListPath =
                    Path.Combine(outputPath, "index.m3u8"),

                Varints = new List<VarintInfo>()
            };
        }
        private string BuildFfmpegArgs(
            string input,
            string outputDir)
        {
            var outputPath =
                Path.Combine(outputDir, "index.m3u8");

            return
                $"-i \"{input}\" " +
                "-codec:v libx264 " +
                "-codec:a aac " +
                "-preset fast " +
                "-g 48 " +
                "-sc_threshold 0 " +
                "-hls_time 6 " +
                "-hls_playlist_type vod " +
                $"-hls_segment_filename \"{outputDir}/segment_%03d.ts\" " +
                $"\"{outputPath}\"";
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
