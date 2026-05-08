using TranscodeVideoServices.Models;
using TranscodeVideoServices.Models.Enums;

namespace TranscodeVideoServices.Services
{
    public interface IVideoService
    {
        Task<Guid> UploadAsync(Stream File, string title, string description);
        Task<VideoDto> GetByIdAsync(Guid videoId);
        Task<IEnumerable<VideoDto>> GetAllAsync();
        Task<string> GetPlaybackUrlAsync(Guid videoId);
        Task RetryProcessingAsync(Guid videoId);
    }
    public class VideoService : IVideoService
    {
        private readonly IJobQueue _jobQueue;
        private readonly IVideoRepositary _videoRepositary;
        private IProcessingJobRepositary _processingJobRepoistary;
        private readonly IBlobService _blobservices;

        public VideoService(IJobQueue queue, IProcessingJobRepositary processingJobRepositary, IVideoRepositary videoRepositary,IBlobService blobServices)
        {
            this._jobQueue = queue;
            this._processingJobRepoistary = processingJobRepositary;
            this._videoRepositary = videoRepositary;
            this._blobservices = blobServices;
        }


        public async Task<Guid> UploadAsync(Stream File, string title, string description)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))

                throw new ArgumentException("Invalid metadata");

            if (File == null || !File.CanRead || File.Length == 0)
                throw new ArgumentException("Invalid file");

            var video = new Video(title, description);

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "storage",
                "raw",
                $"{video.Id}.mp4");

            await _blobservices.UploadAsync(path, File);

            video.SetFilePath(path);

            await _videoRepositary.AddAsync(video);

            var job = new ProcessingJob(video.Id, path);

            await _processingJobRepoistary.AddAsync(job);

            await _jobQueue.EnqueueAsync(job.Id);

            return video.Id;
        }


        public async Task<string> GetPlaybackUrlAsync(Guid videoId)
        {
            if (videoId == Guid.Empty)
            {
                throw new ArgumentException("Invalid VideoId");
            }

            var video = await _videoRepositary.GetByIdAsync(videoId);

            if (video == null) throw new Exception("not found");

            if (video.Status != VideoStatus.Ready)
                throw new InvalidOperationException("Video not ready for playback");

            return video.MasterPlayListFilePath;


        }

        public async Task<VideoDto> GetByIdAsync(Guid videoId)
        {
            if (Guid.Empty == videoId)
            {
                throw new ArgumentException("videoId is invalid");

            }

            var video = await _videoRepositary.GetByIdAsync(videoId);

            if (video == null) throw new SystemException("video is not found");

            var data = new VideoDto()
            {
                Description = video.Description,
                Id = video.Id,
                Title = video.Title,
                PlaybackUrl = $"/storage/hls/{video.Id}/index.m3u8",
                Duration = video.Duration


            };
            return data;
        }


        public async Task<IEnumerable<VideoDto>> GetAllAsync()
        {

            var videos = await _videoRepositary.GetAllAsync();

            if (videos == null) throw new SystemException("no videos found");

            return videos.Select(videos => new VideoDto
            {
                Id = videos.Id,
                Title = videos.Title,
                Description = videos.Description,
                Duration = videos.Duration,
                PlaybackUrl = $"/storage/hls/{videos.Id}/index.m3u8"
            }).ToList(); ;


        }



        public async Task RetryProcessingAsync(Guid videoId)
        {

            if (Guid.Empty == videoId) throw new ArgumentException("videoid is invalid");
            var video = await _videoRepositary.GetByIdAsync(videoId);
            if (video == null) throw new InvalidOperationException("Video not found");
            if (video.Status != VideoStatus.Failed) throw new InvalidOperationException("only failed videos can be retried");
            var job = await _processingJobRepoistary.GetByIdAsync(videoId);
            if (job == null) throw new InvalidOperationException("processing jobs not found");

            if (job.RetryCount >= job.MaxRetryCount)
            {
                throw new InvalidOperationException("Rate Limit reached");
            }

            job.IncrementRetry("manual eror");

            await _processingJobRepoistary.UpdateAsync(job);

            await _jobQueue.EnqueueAsync(job.Id);


        }

        
    }

}
