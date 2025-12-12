namespace Auth.Application.Abstractions.Interfaces.Caching
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        Task SetAsync<T>( string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

        Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    }
}
