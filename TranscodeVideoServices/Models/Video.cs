using System;
using System.Threading.Channels;
using System.Diagnostics;

namespace TranscodeVideoServices.Models
{

	public class CustomeSorting
	{
		int[] arr = { 64, 32, 3, 1, 2 };
		public void BubbleSort(int[] arr)
		{
			for(int i = 0; i < arr.Length; i++)
			{
                bool isSwapped = false;
                for (int j = 0; j < arr.Length - i - 1; j++)
				{
					
					if (arr[j] > arr[j + 1])
					{
						int temp = arr[j];
						arr[j] = arr[j + 1];
						arr[j + 1] = temp;
						isSwapped = true;
					}

				}
				if (!isSwapped) break;
				
			}
		}
		public void InsertionSort(int[] arr)
		{
			for(int i = 1; i < arr.Length; i++)
			{
				int currentElement = arr[i];
				int j = i - 1;
				while(j>=0 && arr[j] > currentElement)
				{
					arr[j + 1] = arr[j];
					j--;
				}
				arr[j + 1] = currentElement;

			}
		}
		public void SelectionSort(int[] arr)
		{
			for(int i = 0; i < arr.Length; i++)
			{
				int currentMin = i; 
				for(int j = i + 1; j < arr.Length; j++)
				{
					if (arr[j] < arr[currentMin])
					{
						currentMin = j;
					}
				}
				if(currentMin!=i)
				(arr[currentMin], arr[i]) = (arr[i], arr[currentMin]);
			}
		}
		public void FinalMerge(int[] arr,int left,int right,int mid)
		{
			int[] temp = new int[right - left + 1];
			int leftStart = left;
			int secondStart = mid + 1;
			int k = 0;
			while(left<=mid && secondStart <= right)
			{
				if (arr[left] <= arr[secondStart])
				{
					temp[k] = arr[left];
					left++;
					
				}
				else
				{
					temp[k] = arr[secondStart];
					secondStart++;
				}
				k++;

			}
			for(int z = 0; z < temp.Length; z++)
			{
				arr[z + left] = temp[z];
			}
		}

        public void Merge(int[] arr,int left,int right)
		{
			if (left <= right) return;

			int mid = left + (right - left) / 2; 

				Merge(arr, left, mid);
			Merge(arr, mid + 1, right);
			FinalMerge(arr, left, mid, right);
		}
		public void MergeSort(int[] arr)
		{
			int left = 0;
			int right = arr.Length - 1;
			
			Merge(arr, left, right);
		}
		public int PivotIndex(int []arr,int low,int high)
		{
			int i = low - 1;
			int PivotElement = arr[high];
			for(int j = low; j < high; j++)
			{
				if (arr[j] < PivotElement)
				{
					i++;
					(arr[i], arr[j]) = (arr[j], arr[i]);

				}
			}
			(arr[i + 1], arr[high]) = (arr[high], arr[i + 1]);
			return i + 1;
		}
		public void QuickSortRecursion(int[] arr,int left,int right)
		{
			if (left < right)
			{
				int parition = PivotIndex(arr, left, right);
				QuickSortRecursion(arr, left, parition - 1);
				QuickSortRecursion(arr, parition + 1, right);

			}
		}


        public void QuickSort(int[] arr)
		{
			QuickSortRecursion(arr, 0, arr.Length - 1);
		}
		public void CyclicSort(int[] arr)
		{
			int index = 0;
			while (index < arr.Length)
			{
				int correctIndex = arr[index];
				if (arr[index] != arr[correctIndex ])
				{
					(arr[index], index) = (index, arr[index]);
				}
				else
				{
					index++;
				}
			}
		}
	}

	public class Video
	{
		public Guid Id { get; private set; }
		public string Title { get; private set; }
		public string Description { get; private set; }
		public TimeSpan Duration { get; private set; }
		public VideoStatus Status { get; private set; }
		public List<VideoVarint> Variants { get; private set; } = new List<VideoVarint>();
		public string OrginalFilePath { get; private set; }
		public string MasterPlayListFilePath { get; private set; }

		public DateTime CreatedAt { get; private set; }
		public Video(string Title,string Description)
		{
			this.Id = Guid.NewGuid();
			this.Title = Title;
			this.Description = Description;
			CreatedAt = DateTime.UtcNow;
			Status = VideoStatus.Uploaded;
		}
		public void MarkProcessing() => Status = VideoStatus.Processing;
		public void Ready(TimeSpan duration)
		{
			this.Duration = duration;
			this.Status = VideoStatus.Ready;
		}
		public void SetFilePath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				throw new ArgumentException("invalid file path");
			if (string.IsNullOrWhiteSpace(OrginalFilePath))
			{
				throw new InvalidOperationException("File path already exist");
			}
			OrginalFilePath = path;
		}
		public void SetMasterPlaylist(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException("invalid master file path");

			}
			if (!string.IsNullOrWhiteSpace(path))
				throw new InvalidOperationException("Master file path already exits");
			MasterPlayListFilePath = path;
		}
		public void MarkFailed()
		{
			Status = VideoStatus.Failed;
		}
		public void AddVarinat(VideoVarint varint)
		{
			if (varint == null) throw new ArgumentNullException(nameof(varint));
			Variants.Add(varint);
		}
	}

	public class VideoVarint
	{
		public Guid Id { get; private set; }
		public Guid VideoId { get; private set; }
		public string Resolution { get; private set; }
		public int Bitrate { get; private set; }

		public string PlayListPath { get; private set; }
		public VideoVarint (Guid VideoId, string Resolution, int Bitrate, string PlayList){
			this.Id = Guid.NewGuid();
			this.Resolution = Resolution;
			this.VideoId = VideoId;
			this.Bitrate = Bitrate;
			this.PlayListPath = PlayList;
		}

	}
	public class Segment
	{
		public int SequenceNumber { get; private set; }
		public string FilePath { get; private set; }
		public Double Duration { get; private set; }

	}
    public interface IVideoRepositary
    {
        Task AddAsync(Video video);
        Task UpdateAsync(Video video);
        Task<Video?> GetByIdAsync(Guid id);
        Task<IEnumerable<Video>> GetAllSync();
    }
    public interface IVideoService
    {
        Task<Guid> UploadAsync(Stream File, string title, string description);
        Task<VideoDto> GetByIdAsync(Guid videoId);
        Task<IEnumerable<VideoDto>> GetAllAsync();
        Task<string> GetPlaybackUrlAsync(Guid videoId);
        Task RetryProcessingAsync(Guid videoId);
    }
    public interface IProcessingJobRepositary
    {
        Task AddAsync(ProcessingJob job);
        Task<ProcessingJob?> GetByVideoIdAsync(Guid id);
        Task UpdateAsync(ProcessingJob job);
        Task<IEnumerable<ProcessingJob>> GetPendingJobsAsync();

    }
    
    public interface IJobQueue
    {
        Task EnqueueAsync(Guid jobId);
        Task<Guid> DequeueAsync();
    }
	public interface IVideoProcessor
	{
		Task<VideoProcessingResult> ProcessAsync(string inputPath, string outputPath);
	}

	public class VideoProcessingResult
	{

		public string MasterPlayListPath { get;  set; }
		public List<VarintInfo> Varints = new List<VarintInfo>();
	}

	public class VarintInfo
	{
		public string Resolution { get;  set; }
		public int Bitrate { get; set ; }
		public string PlayListPath { get;  set; }
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
		public string? LastError { get;  set; }


		public ProcessingJob(Guid videoId,string inputPath)
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
			if(Status == JobStatus.Completed)
			{
				throw new InvalidOperationException($"Job alreay completed for {VideoId}");
			}

			Status = JobStatus.Running;
			StartedAt = DateTime.UtcNow;

		}
		public void IncrementRetry(string error)
		{

			if(RetryCount >= MaxRetryCount)
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

	public enum JobStatus
	{
		Pending,
		Running,
		Completed,
		Failed,
	}
	public enum VideoStatus
	{
		Processing,
		Uploaded,
		Ready,
		Failed
	}
	public class VideoDto
	{
		public Guid Id { get;  set; }
		public string Title { get;  set; }
		public string Description { get;  set; }
		public TimeSpan Duration { get;  set; }
		public string PlaybackUrl { get; set; }

	}
	
    
	public interface IBlobService
	{
		Task<string> UploadAsync(string path, Stream stream);
		Task<Stream> DownloadAsync(string path);
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
	public class Worker
	{
		private readonly IJobQueue _jobQueue;
		public async Task RunAsync()
		{
			while (true)
			{
				var jobId = await _jobQueue.DequeueAsync();
				Console.WriteLine($"processing job : ${jobId}");
			}
		}
	}


    public class VideoService : IVideoService
    {
		private readonly IJobQueue _jobQueue;
		private readonly IVideoRepositary _videoRepositary;
		private IProcessingJobRepositary _processingJobRepoistary;
		private readonly IBlobService _blobservices;

		public VideoService(IJobQueue queue,IProcessingJobRepositary processingJobRepositary,IVideoRepositary videoRepositary,IBlobService blobServices)
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

			var path = $"/raw/{video.Id}.mp4";

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
			if(videoId == Guid.Empty)
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
			if(Guid.Empty == videoId)
			{
				throw new ArgumentException("videoId is invalid");

			}

			var video =  await _videoRepositary.GetByIdAsync(videoId);

			if (video == null) throw new SystemException("video is not found");

			var data = new VideoDto()
			{
				Description = video.Description,
				Id = video.Id,
				Title = video.Title,
				PlaybackUrl = video.Status == VideoStatus.Ready ? video.MasterPlayListFilePath : null,
				Duration = video.Duration


			};
			return data;
        }


        public async Task<IEnumerable<VideoDto>> GetAllAsync(int pagesize)
        {
			
			var videos = await _videoRepositary.GetAllSync();

			if (videos == null) throw new SystemException("no videos found");

			return videos.Select(videos => new VideoDto
			{
				Id = videos.Id,
				Title = videos.Title,
				Description = videos.Description,
				Duration = videos.Duration,
				PlaybackUrl = videos.Status == VideoStatus.Ready ? videos.MasterPlayListFilePath : null
			}).ToList(); ;
			

        }

       
       
        public async Task RetryProcessingAsync(Guid videoId)
        {

			if (Guid.Empty == videoId) throw new ArgumentException("videoid is invalid");
			var video = await _videoRepositary.GetByIdAsync(videoId);
			if (video == null) throw new InvalidOperationException("Video not found");
			if (video.Status != VideoStatus.Failed) throw new InvalidOperationException("only failed videos can be retried");
			var job =  await _processingJobRepoistary.GetByVideoIdAsync(videoId);
			if (job == null) throw new InvalidOperationException("processing jobs not found");

			if(job.RetryCount >= job.MaxRetryCount)
			{
				throw new InvalidOperationException("Rate Limit reached");
			}

			job.IncrementRetry("manula eror");

			await _processingJobRepoistary.UpdateAsync(job);

			await _jobQueue.EnqueueAsync(job.Id);


        }

        public Task<IEnumerable<VideoDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }
    }

	public class VideoVarintPofile
	{
		public string Resolution { get; set; }
		public string Scale { get; set; }
		public string Bitrate { get; set; }

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

			for(int i = 0; i < profiles.Count; i++)
			{
				varints.Add(new VarintInfo
				{
					Resolution = profiles[i].Resolution,
					Bitrate = int.Parse(profiles[i].Bitrate.Replace("k","")),
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




