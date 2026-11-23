using System.Linq.Expressions;
using InterCase.Repository.Context;
using InterCase.Repository.Entities;
using InterCase.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InterCase.Repository.Repositories
{
    public abstract class BaseRepository<TEntity>(AppDbContext context) : IBaseRepository<TEntity>
        where TEntity : BaseEntity
    {
        protected AppDbContext Context { get; } = context;

        protected virtual IQueryable<TEntity> Query(bool trackChanges)
        {
            var query = Context.Set<TEntity>().Where(e => !e.IsDeleted);
            return trackChanges ? query : query.AsNoTracking();
        }

        public Task<TEntity?> GetByIdAsync(long id, bool trackChanges, CancellationToken ct = default) =>
            Query(trackChanges).FirstOrDefaultAsync(e => e.Id == id, ct);

        public Task<TEntity?> GetByPublicIdAsync(Guid publicId, bool trackChanges, CancellationToken ct = default) =>
            Query(trackChanges).FirstOrDefaultAsync(e => e.PublicId == publicId, ct);

        public Task<List<TEntity>> GetAllAsync(bool trackChanges, CancellationToken ct = default) =>
            Query(trackChanges).OrderBy(e => e.Id).ToListAsync(ct);

        public IQueryable<TEntity> FindByCondition(Expression<Func<TEntity, bool>> predicate, bool trackChanges) =>
            Query(trackChanges).Where(predicate);

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
            Query(trackChanges: false).AnyAsync(predicate, ct);

        public async Task AddAsync(TEntity entity, CancellationToken ct = default) =>
            await Context.Set<TEntity>().AddAsync(entity, ct);

        public void Update(TEntity entity) => Context.Set<TEntity>().Update(entity);

        public void Delete(TEntity entity) => Context.Set<TEntity>().Remove(entity);

        protected static async Task<(List<TEntity> Items, int TotalCount)> ToPageAsync(
            IOrderedQueryable<TEntity> query,
            int page,
            int pageSize,
            CancellationToken ct)
        {
            var totalCount = await query.CountAsync(ct);
            var items = totalCount == 0
                ? []
                : await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return (items, totalCount);
        }
    }
}
