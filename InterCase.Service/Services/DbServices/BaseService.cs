using AutoMapper;
using InterCase.Common;
using InterCase.Repository.Entities;
using InterCase.Repository.Interfaces;
using InterCase.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace InterCase.Service.Services.DbServices
{
    public abstract class BaseService<TEntity, TResponse, TCreateRequest, TUpdateRequest>(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        ILogger logger) : IBaseService<TResponse, TCreateRequest, TUpdateRequest>
        where TEntity : BaseEntity
    {
        protected IRepositoryManager RepositoryManager { get; } = repositoryManager;
        protected IMapper Mapper { get; } = mapper;
        protected ILogger Logger { get; } = logger;

        protected abstract IBaseRepository<TEntity> Repository { get; }

        protected virtual TResponse ToResponse(TEntity entity) => Mapper.Map<TResponse>(entity);
        protected virtual TEntity ToEntity(TCreateRequest request) => Mapper.Map<TEntity>(request);
        protected virtual void Apply(TUpdateRequest request, TEntity entity) => Mapper.Map(request, entity);

        protected virtual void EnsureCanUpdate(TEntity entity)
        {
        }

        protected virtual void EnsureCanDelete(TEntity entity)
        {
        }

        protected virtual Task OnCreatingAsync(TEntity entity, TCreateRequest request, CancellationToken ct) => Task.CompletedTask;
        protected virtual Task OnCreatedAsync(TEntity entity, CancellationToken ct) => Task.CompletedTask;
        protected virtual Task OnUpdatedAsync(TEntity entity, CancellationToken ct) => Task.CompletedTask;
        protected virtual Task OnDeletedAsync(TEntity entity, CancellationToken ct) => Task.CompletedTask;

        public virtual async Task<TResponse?> GetByIdAsync(long id, CancellationToken ct = default)
        {
            var entity = await Repository.GetByIdAsync(id, trackChanges: false, ct);
            return entity is null ? default : ToResponse(entity);
        }

        public virtual async Task<TResponse?> GetByPublicIdAsync(Guid publicId, CancellationToken ct = default)
        {
            var entity = await Repository.GetByPublicIdAsync(publicId, trackChanges: false, ct);
            return entity is null ? default : ToResponse(entity);
        }

        public virtual async Task<IReadOnlyList<TResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var entities = await Repository.GetAllAsync(trackChanges: false, ct);
            return entities.Select(ToResponse).ToList();
        }

        public virtual async Task<TResponse> CreateAsync(TCreateRequest request, CancellationToken ct = default)
        {
            var entity = ToEntity(request);
            await OnCreatingAsync(entity, request, ct);
            await Repository.AddAsync(entity, ct);
            await RepositoryManager.SaveChangesAsync(ct);

            await OnCreatedAsync(entity, ct);
            return ToResponse(entity);
        }

        public virtual async Task<TResponse> UpdateAsync(Guid publicId, TUpdateRequest request, CancellationToken ct = default)
        {
            var entity = await GetTrackedOrThrowAsync(publicId, ct);
            EnsureCanUpdate(entity);
            Apply(request, entity);
            await RepositoryManager.SaveChangesAsync(ct);

            await OnUpdatedAsync(entity, ct);
            return ToResponse(entity);
        }

        public virtual async Task DeleteAsync(Guid publicId, CancellationToken ct = default)
        {
            var entity = await GetTrackedOrThrowAsync(publicId, ct);
            EnsureCanDelete(entity);
            Repository.Delete(entity);
            await RepositoryManager.SaveChangesAsync(ct);

            await OnDeletedAsync(entity, ct);
        }

        private async Task<TEntity> GetTrackedOrThrowAsync(Guid publicId, CancellationToken ct) =>
            await Repository.GetByPublicIdAsync(publicId, trackChanges: true, ct)
            ?? throw new NotFoundException($"{typeof(TEntity).Name} {publicId} was not found.");
    }
}
