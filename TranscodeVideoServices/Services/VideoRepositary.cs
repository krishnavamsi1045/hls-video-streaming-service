using TranscodeVideoServices.Models;

namespace TranscodeVideoServices.Services
{
    public interface IVideoRepositary
    {
        Task AddAsync(Video video);
        Task UpdateAsync(Video video);
        Task<Video?> GetByIdAsync(Guid id);
        Task<IEnumerable<Video>> GetAllAsync();
    }

    public class InMemoryVideoRepositary : IVideoRepositary
    {
        public readonly List<Video> _videos = new();
        public Task<IEnumerable<Video>> GetAllAsync()
        {
            return Task.FromResult(_videos.AsEnumerable());
        }
        public Task AddAsync(Video video)
        {
            _videos.Add(video);

            return Task.CompletedTask;
        }

        public Task UpdateAsync(Video video)
        {
            var existingVideo = _videos.FirstOrDefault(item => item.Id == video.Id);

            if(existingVideo == null)
            {
                throw new InvalidOperationException("Video not found");

            }
            _videos.Remove(existingVideo);

            _videos.Add(video);


            return Task.CompletedTask;
        }

        public Task<Video?> GetByIdAsync(Guid id)
        {
            var video = _videos.FirstOrDefault(item => item.Id == id);

            return Task.FromResult(video);
        }
    }
}
