using System.Linq.Expressions;
using InterCase.Repository.Entities;

namespace InterCase.Repository.Interfaces
{
    public interface IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        Task<TEntity?> GetByIdAsync(long id, bool trackChanges, CancellationToken ct = default);
        Task<TEntity?> GetByPublicIdAsync(Guid publicId, bool trackChanges, CancellationToken ct = default);
        Task<List<TEntity>> GetAllAsync(bool trackChanges, CancellationToken ct = default);
        IQueryable<TEntity> FindByCondition(Expression<Func<TEntity, bool>> predicate, bool trackChanges);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
        Task AddAsync(TEntity entity, CancellationToken ct = default);
        void Update(TEntity entity);
        void Delete(TEntity entity);
    }
}
