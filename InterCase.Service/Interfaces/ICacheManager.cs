namespace InterCase.Service.Interfaces
{
    public interface ICacheManager
    {
        Task<byte[]?> GetAsync(string key);
        Task SetAsync(string key, byte[] value, TimeSpan? ttl = null);
        Task RemoveAsync(string key);
    }
}
