namespace InterCase.Service.Interfaces
{
    public interface IBaseService<TResponse, TCreateRequest, TUpdateRequest>
    {
        Task<TResponse?> GetByIdAsync(long id, CancellationToken ct = default);
        Task<TResponse?> GetByPublicIdAsync(Guid publicId, CancellationToken ct = default);
        Task<IReadOnlyList<TResponse>> GetAllAsync(CancellationToken ct = default);
        Task<TResponse> CreateAsync(TCreateRequest request, CancellationToken ct = default);
        Task<TResponse> UpdateAsync(Guid publicId, TUpdateRequest request, CancellationToken ct = default);
        Task DeleteAsync(Guid publicId, CancellationToken ct = default);
    }
}
