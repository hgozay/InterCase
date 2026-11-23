using InterCase.Repository.Context;
using InterCase.Repository.Entities;
using InterCase.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InterCase.Repository.Repositories
{
    public sealed class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
    {
        public Task<User?> GetByUsernameAsync(string username, bool trackChanges, CancellationToken ct = default) =>
            Query(trackChanges).FirstOrDefaultAsync(u => u.Username == username, ct);

        public Task<List<User>> GetDemoUsersAsync(CancellationToken ct = default) =>
            Query(trackChanges: false)
                .Where(u => u.IsDemo)
                .OrderBy(u => u.Username)
                .ToListAsync(ct);
    }
}
