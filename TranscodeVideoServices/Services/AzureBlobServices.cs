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