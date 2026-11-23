using InterCase.Repository.Context;
using InterCase.Repository.Entities;

namespace InterCase.Repository.Repositories
{
    public sealed class UserLoginRepository(AppDbContext context) : BaseRepository<UserLogin>(context)
    {
    }
}
