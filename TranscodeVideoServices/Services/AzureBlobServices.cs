using Azure.Storage.Blobs;
using TranscodeVideoServices.Models;

public class AzureBlobService : IBlobService
{
    private readonly BlobContainerClient _container;

    public AzureBlobService(string connectionString)
    {
        var serviceClient = new BlobServiceClient(connectionString);
        _container = serviceClient.GetBlobContainerClient("videos");
    }

    public async Task<string> UploadAsync(string path, Stream stream)
    {
        var blobClient = _container.GetBlobClient(path);

        await blobClient.UploadAsync(stream, overwrite: true);

        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadAsync(string path)
    {
        var blobClient = _container.GetBlobClient(path);

        var response = await blobClient.DownloadAsync();

        return response.Value.Content;
    }
}

public class LocalBlobSerivce : IBlobService
{
    private readonly string _basePath;

    public LocalBlobSerivce()
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "storage");
        
    }
    public async Task<string> UploadAsync(string path, Stream stream)
    {
        var filePath = Path.Combine(_basePath, path);
        var directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(filePath);
        }

        using FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

      await  stream.CopyToAsync(fileStream);
      return filePath;

    }

    public async Task<Stream> DownloadAsync(string path)
    {
        var filePath = Path.Combine(_basePath, path);
        var memory = new MemoryStream();

        using var file = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        await file.CopyToAsync(memory);

        memory.Position = 0;
        return memory;
    }
}