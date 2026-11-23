using InterCase.Repository.Context;
using InterCase.Repository.Entities;

namespace InterCase.Repository.Repositories
{
    public sealed class NotificationRepository(AppDbContext context)
        : BaseRepository<Notification>(context)
    {
    }
}
