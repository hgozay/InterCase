using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InterCase.Repository.Entities
{
    [Table("users")]
    [Index(nameof(Username), IsUnique = true)]
    public class User : BaseEntity
    {
        [MaxLength(64)]
        public required string Username { get; set; }

        [MaxLength(128)]
        public required string DisplayName { get; set; }

        [MaxLength(256)]
        public required string PasswordHash { get; set; }

        public bool IsDemo { get; set; }

        public ICollection<Order> Orders { get; set; } = [];

        public ICollection<UserLogin> Logins { get; set; } = [];
    }
}
