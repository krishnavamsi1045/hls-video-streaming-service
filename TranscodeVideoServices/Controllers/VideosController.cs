using Microsoft.AspNetCore.Mvc;
using TranscodeVideoServices.Services;

namespace TranscodeVideoServices.Controllers
{
    [ApiController]
    [Route("api/v1/Videos")]
    public class VideosController : Controller
    {
        private readonly IVideoService _videoService;
        public VideosController(IVideoService videoService)
        {
            _videoService = videoService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> upload(IFormFile file, [FromForm] string title, [FromForm] string description)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file");

            using var stream = file.OpenReadStream();

                var videoId = await _videoService.UploadAsync(stream, title, description);
            return Accepted(new {VideoId = videoId ,message = "Video uploaded successfully"});
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var video = await _videoService.GetByIdAsync(id);

            return Ok(video);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var videos = await _videoService.GetAllAsync();
            return Ok(videos);
        }

        [HttpGet("{Id}/playback")]
        public async Task<IActionResult> GetPlayback(Guid id)
        {
            var url = await _videoService.GetPlaybackUrlAsync(id);
            return Ok(new
            {
                PlaybackUrl = url
            });


        }

        [HttpGet("{id}/retry")]
        public async Task<IActionResult> Retry(Guid id)
        {
            await _videoService.RetryProcessingAsync(id);

            return Ok(new
            {
                Message = "Retry Queued"
            });
        }
        

    }
}
