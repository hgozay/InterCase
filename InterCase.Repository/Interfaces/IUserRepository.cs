using InterCase.Repository.Entities;

namespace InterCase.Repository.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username, bool trackChanges, CancellationToken ct = default);
        Task<List<User>> GetDemoUsersAsync(CancellationToken ct = default);
    }
}
