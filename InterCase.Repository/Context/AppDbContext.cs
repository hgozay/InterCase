using InterCase.Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace InterCase.Repository.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<UserLogin> UserLogins => Set<UserLogin>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Notification> Notifications => Set<Notification>();
    }
}
