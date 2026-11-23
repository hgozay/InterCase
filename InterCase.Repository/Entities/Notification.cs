using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InterCase.Repository.Entities
{
    [Table("notifications")]
    public class Notification : BaseEntity
    {
        public long OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Order Order { get; set; } = null!;

        [MaxLength(64)]
        public required string UserId { get; set; }

        [MaxLength(32)]
        public required string Channel { get; set; }

        [MaxLength(500)]
        public required string Message { get; set; }

        public DateTime SentAt { get; set; }
    }
}
