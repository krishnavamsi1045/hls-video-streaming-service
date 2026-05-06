using Azure.Storage.Blobs;
using TranscodeVideoServices.Models;
using TranscodeVideoServices.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = builder.Configuration.GetConnectionString("AzureStorage");
builder.Services.AddSingleton<IBlobService>(new AzureBlobService(connectionString));
builder.Services.AddSingleton<IJobQueue, InMemoryJobQueue>();
builder.Services.AddSingleton<IVideoRepositary, InMemoryVideoRepositary>();
builder.Services.AddSingleton<IProcessingJobRepositary, InMemoryProcessingJobRepositary>();
builder.Services.AddScoped<IVideoProcessor, FFmpegVideoProcessor>();
builder.Services.AddScoped<IVideoService, VideoService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

