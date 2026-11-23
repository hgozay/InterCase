namespace InterCase.Service.Interfaces
{
    public interface IBaseService<TResponse, TRequest>
    {
        Task<TResponse?> GetByIdAsync(long id, CancellationToken ct = default);
        Task<TResponse?> GetByPublicIdAsync(Guid publicId, CancellationToken ct = default);
        Task<IReadOnlyList<TResponse>> GetAllAsync(CancellationToken ct = default);
        Task<TResponse> CreateAsync(TRequest request, CancellationToken ct = default);
        Task<TResponse> UpdateAsync(Guid publicId, TRequest request, CancellationToken ct = default);
        Task DeleteAsync(Guid publicId, CancellationToken ct = default);
    }
}
