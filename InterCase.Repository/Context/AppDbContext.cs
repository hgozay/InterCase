using InterCase.Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace InterCase.Repository.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Notification> Notifications => Set<Notification>();
    }
}
