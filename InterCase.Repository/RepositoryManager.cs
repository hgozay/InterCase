using InterCase.Repository.Context;
using InterCase.Repository.Entities;
using InterCase.Repository.Interfaces;
using InterCase.Repository.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace InterCase.Repository
{
    public sealed class RepositoryManager : IRepositoryManager
    {
        private readonly AppDbContext _context;
        private readonly Lazy<IUserRepository> _user;
        private readonly Lazy<IBaseRepository<UserLogin>> _userLogin;
        private readonly Lazy<IOrderRepository> _order;
        private readonly Lazy<IBaseRepository<Notification>> _notification;
        private IDbContextTransaction? _transaction;

        public RepositoryManager(AppDbContext context)
        {
            _context = context;
            _user = new Lazy<IUserRepository>(() => new UserRepository(context));
            _userLogin = new Lazy<IBaseRepository<UserLogin>>(() => new UserLoginRepository(context));
            _order = new Lazy<IOrderRepository>(() => new OrderRepository(context));
            _notification = new Lazy<IBaseRepository<Notification>>(() => new NotificationRepository(context));
        }

        public IUserRepository User => _user.Value;
        public IBaseRepository<UserLogin> UserLogin => _userLogin.Value;
        public IOrderRepository Order => _order.Value;
        public IBaseRepository<Notification> Notification => _notification.Value;

        public int SaveChanges()
        {
            ApplyAuditInfo();
            return _context.SaveChanges();
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            ApplyAuditInfo();
            return _context.SaveChangesAsync(ct);
        }

        public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            if (_transaction is not null)
                throw new InvalidOperationException("A transaction is already in progress.");

            _transaction = await _context.Database.BeginTransactionAsync(ct);
        }

        public async Task CommitTransactionAsync(CancellationToken ct = default)
        {
            if (_transaction is null)
                throw new InvalidOperationException("There is no active transaction to commit.");

            try
            {
                await _transaction.CommitAsync(ct);
            }
            catch
            {
                _context.ChangeTracker.Clear();
                throw;
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction is null)
                return;

            try
            {
                await _transaction.RollbackAsync(CancellationToken.None);
            }
            finally
            {
                _context.ChangeTracker.Clear();
                await DisposeTransactionAsync();
            }
        }

        private void ApplyAuditInfo()
        {
            var now = DateTime.UtcNow;

            foreach (var entry in _context.ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        if (entry.Entity.PublicId == Guid.Empty)
                            entry.Entity.PublicId = Guid.CreateVersion7();
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        entry.Property(x => x.CreatedAt).IsModified = false;
                        entry.Property(x => x.PublicId).IsModified = false;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = now;
                        break;
                }
            }
        }

        private async Task DisposeTransactionAsync()
        {
            if (_transaction is null)
                return;

            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
