using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InterCase.Common;
using Microsoft.EntityFrameworkCore;

namespace InterCase.Repository.Entities
{
    [Table("orders")]
    [Index(nameof(UserId), nameof(CreatedAt))]
    public class Order : BaseEntity
    {
        [MaxLength(64)]
        public required string UserId { get; set; }

        [MaxLength(64)]
        public required string ProductId { get; set; }

        public int Quantity { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime? ProcessedAt { get; set; }

        public ICollection<Notification> Notifications { get; set; } = [];
    }
}
