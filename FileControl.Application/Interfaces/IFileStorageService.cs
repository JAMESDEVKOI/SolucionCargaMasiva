namespace FileControl.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(
            Stream fileStream,
            string fileName,
            CancellationToken cancellationToken = default);

        Task<Stream> DownloadFileAsync(
            string fileId,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteFileAsync(
            string fileId,
            CancellationToken cancellationToken = default);
    }
}
