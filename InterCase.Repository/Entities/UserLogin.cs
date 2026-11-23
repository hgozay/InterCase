using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InterCase.Repository.Entities
{
    [Table("user_logins")]
    public class UserLogin : BaseEntity
    {
        public long UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public User User { get; set; } = null!;

        public DateTime LoggedInAt { get; set; }

        [MaxLength(64)]
        public required string TokenId { get; set; }

        public required string AccessToken { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
