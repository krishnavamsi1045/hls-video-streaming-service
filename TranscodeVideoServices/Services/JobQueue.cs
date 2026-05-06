using System.Threading.Channels;

namespace TranscodeVideoServices.Services
{
    public interface IJobQueue
    {
        Task EnqueueAsync(Guid jobId);
        Task<Guid> DequeueAsync();
    }


    public class InMemoryJobQueue : IJobQueue
    {
        private readonly Channel<Guid> _queue = Channel.CreateUnbounded<Guid>();

        public async Task EnqueueAsync(Guid jobId)
        {
            await _queue.Writer.WriteAsync(jobId);
        }
        public async Task<Guid> DequeueAsync()
        {
            return await _queue.Reader.ReadAsync();
        }

    }
}
