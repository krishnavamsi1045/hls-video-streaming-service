using TranscodeVideoServices.Models.Enums;

namespace TranscodeVideoServices.Models
{
    public interface IProcessingJobRepositary
    {
        Task AddAsync(ProcessingJob job);
        Task<ProcessingJob?> GetByVideoIdAsync(Guid id);
        Task UpdateAsync(ProcessingJob job);
        Task<IEnumerable<ProcessingJob>> GetPendingJobsAsync();

    }

    public class InMemoryProcessingJobRepositary : IProcessingJobRepositary
    {
        private readonly List<ProcessingJob> processingJobs = new List<ProcessingJob>();

        public Task AddAsync(ProcessingJob job)
        {
            processingJobs.Add(job);
            return Task.CompletedTask;
        }

        public Task<ProcessingJob> GetByVideoIdAsync(Guid id)
        {
            var job = processingJobs.FirstOrDefault(j => j.VideoId == id);
            if (job == null)
                throw new InvalidOperationException("Job not found for video id: " + id);

            return Task.FromResult(job);
        }

        public Task UpdateAsync(ProcessingJob job)
        {
            var existingJob = processingJobs.FirstOrDefault(j => j.Id == job.Id);
            if (existingJob == null)
                throw new InvalidOperationException("Job not found for id: " + job.Id);
            processingJobs.Remove(existingJob);
            processingJobs.Add(job);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<ProcessingJob>> GetPendingJobsAsync()
        {
            var pendingJobs = processingJobs.Where(j => j.Status == JobStatus.Pending).ToList();
            return Task.FromResult(pendingJobs.AsEnumerable());
        }


    }
    public class ProcessingJob
    {
        public Guid Id { get; private set; }
        public Guid VideoId { get; private set; }
        public string InputPath { get; private set; }
        public string OutputPath { get; private set; }
        public DateTime? StartedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public JobStatus Status { get; private set; }
        public int RetryCount { get; private set; }
        public int MaxRetryCount { get; private set; } = 3;
        public string? LastError { get; set; }


        public ProcessingJob(Guid videoId, string inputPath)
        {
            this.InputPath = inputPath;
            this.VideoId = videoId;
            CreatedAt = DateTime.UtcNow;
            Status = JobStatus.Pending;
            Id = Guid.NewGuid();
        }

        public void Start()
        {
            if (Status == JobStatus.Failed && RetryCount >= MaxRetryCount)
                throw new InvalidOperationException("Retries exhausted");
            if (Status == JobStatus.Running)
            {
                throw new InvalidOperationException($"job alaredy running for {VideoId} ");
            }
            if (Status == JobStatus.Completed)
            {
                throw new InvalidOperationException($"Job alreay completed for {VideoId}");
            }

            Status = JobStatus.Running;
            StartedAt = DateTime.UtcNow;

        }
        public void IncrementRetry(string error)
        {

            if (RetryCount >= MaxRetryCount)
            {
                throw new InvalidOperationException("Max retries exceeded");
            }
            RetryCount++;
            LastError = error;
            Status = JobStatus.Pending;
        }
        public void Complete()
        {
            if (Status == JobStatus.Completed)
            {
                throw new InvalidOperationException($"Job alreay completed for {VideoId}");
            }
            if (Status != JobStatus.Running)
                throw new InvalidOperationException("Job must be running to complete");
            Status = JobStatus.Completed;
            CompletedAt = DateTime.UtcNow;
        }
        public void Fail(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                throw new ArgumentException("Error required");
            }

            Status = JobStatus.Failed;
            LastError = error;

        }

    }

}
