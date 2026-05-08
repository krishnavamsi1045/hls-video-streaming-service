using System;
using System.Threading.Channels;
using System.Diagnostics;
using TranscodeVideoServices.Models.Enums;
using TranscodeVideoServices.Services;

namespace TranscodeVideoServices.Models
{


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
			if (!string.IsNullOrWhiteSpace(OrginalFilePath))
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


	public class Worker
	{
		private readonly IJobQueue _jobQueue;
		private readonly IVideoProcessor _videoProcessor;
		private readonly IVideoRepositary _videoRepositary;
		private readonly IProcessingJobRepositary _processingJobRepositary;

		public Worker(IJobQueue queue,IVideoProcessor videoProcessor,IVideoRepositary repo,IProcessingJobRepositary processingJobRepositary)
		{
			this._jobQueue = queue;
			this._videoProcessor = videoProcessor;
			this._videoRepositary = repo;
			this._processingJobRepositary = processingJobRepositary;
		}
		public async Task RunAsync()
		{
			Console.WriteLine("Waiting for the job ");
			while (true)
			{
				try
				{
					var jobId = await _jobQueue.DequeueAsync();

					Console.WriteLine($"processing job : {jobId}");

					var job =
						await _processingJobRepositary
							.GetByIdAsync(jobId);

					if (job == null)
						continue;

					var video =
						await _videoRepositary
							.GetByIdAsync(job.VideoId);

					if (video == null)
						continue;

					Console.WriteLine("before start");

					job.Start();

					Console.WriteLine("after start");

					video.MarkProcessing();

					Console.WriteLine("after mark processing");

					var outputPath = Path.Combine(
						Directory.GetCurrentDirectory(),
						"storage",
						"hls",
						video.Id.ToString());

					Directory.CreateDirectory(outputPath);
					Console.WriteLine("ffmpeg started");
					var result =
						await _videoProcessor.ProcessAsync(
							job.InputPath,
							outputPath);

					Console.WriteLine("ffmpeg done");
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
		}
	}

	public class VideoVarintPofile
	{
		public string Resolution { get; set; }
		public string Scale { get; set; }
		public string Bitrate { get; set; }

	}
}




