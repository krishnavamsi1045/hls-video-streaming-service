using Azure.Storage.Blobs;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using TranscodeVideoServices.Models;
using TranscodeVideoServices.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Services
builder.Services.AddSingleton<IBlobService, LocalBlobSerivce>();
builder.Services.AddSingleton<IJobQueue, InMemoryJobQueue>();
builder.Services.AddSingleton<IVideoRepositary, InMemoryVideoRepositary>();
builder.Services.AddSingleton<IProcessingJobRepositary, InMemoryProcessingJobRepositary>();
builder.Services.AddSingleton<IVideoProcessor, FFmpegVideoProcessor>();

builder.Services.AddScoped<IVideoService, VideoService>();

builder.Services.AddSingleton<Worker>();

// Upload limits
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 500_000_500;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 500_000_000;
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});
var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthorization();

// Default static files
app.UseStaticFiles();

// Storage path
var storagePath = Path.Combine(
    builder.Environment.ContentRootPath,
    "storage");

// MIME mappings
var provider = new FileExtensionContentTypeProvider();

provider.Mappings[".m3u8"] =
    "application/vnd.apple.mpegurl";

provider.Mappings[".ts"] =
    "video/mp2t";

// Expose /storage
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),

    RequestPath = "/storage",

    ContentTypeProvider = provider
});

app.MapControllers();

// Start worker
var worker = app.Services.GetRequiredService<Worker>();

_ = Task.Run(async () =>
{
    await worker.RunAsync();
});

app.Run();