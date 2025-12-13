namespace BulkProcessor.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<Stream> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default);
    }
}
