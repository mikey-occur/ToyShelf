using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.Shelf.Response
{
    public class ShelfResponse
    {
        public Guid Id { get; set; }
        public Guid? PartnerId { get; set; }
        public Guid? StoreId { get; set; }
        public string Code { get; set; } = string.Empty;
        public int Level { get; set; }
        public ShelfStatus Status { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? UnassignedAt { get; set; }
    }
}