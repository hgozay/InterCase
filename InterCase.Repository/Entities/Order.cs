using System.ComponentModel.DataAnnotations.Schema;
using InterCase.Common;
using Microsoft.EntityFrameworkCore;

namespace InterCase.Repository.Entities
{
    [Table("orders")]
    [Index(nameof(UserId), nameof(CreatedAt))]
    public class Order : BaseEntity
    {
        public required Guid UserId { get; set; }

        public required Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime? ProcessedAt { get; set; }

        public ICollection<Notification> Notifications { get; set; } = [];
    }
}
