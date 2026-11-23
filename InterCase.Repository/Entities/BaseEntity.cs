using Microsoft.EntityFrameworkCore;

namespace InterCase.Repository.Entities
{
    [Index(nameof(PublicId), IsUnique = true)]
    public abstract class BaseEntity
    {
        public long Id { get; set; }
        public Guid PublicId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
    }
}
